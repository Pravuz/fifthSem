using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ScadaCommunicationProtocol
{
    public enum ScpTcpPacketTypes { RegRequest=1, RegResponse=64, FileRequest=2, FileResponse=65, Temperature=128, Alarm=129, KeepAlive=130 };
    public class ScpTcpPacket
    {
        private ScpTcpPacketTypes type;
        private byte[] payload;

        /// <summary>
        /// When set to true, message will be broadcasted to all ScpHosts when sent.
        /// Only applicable to Push messages, ignored for other message types.
        /// </summary>
        public bool Broadcast { get; set; }
        /// <summary>
        /// Destination address (Hostname) of the destination.
        /// When in slave mode, this is ignored as the message is always sent to the master.
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Source address (Hostname). Don't set manually, will be provided by ScpHost when message is received.
        /// </summary>
        public string Source { get; set; }
        public byte[] Payload
        {
            get { return payload; }
        }
        public int ID { get; set; }
        public bool IsRequest
        {
            get
            {
                return (byte)type < 64;
            }
        }
        public bool IsResponse
        {
            get
            {
                return ((byte)type >= 64) & ((byte)type <= 127);
            }
        }
        public ScpTcpPacketTypes Type
        {
            get
            {
                return type;
            }
        }
        public ScpTcpPacket Clone()
        {
            ScpTcpPacket clone = new ScpTcpPacket(GetBytes());
            clone.Source = this.Source;
            clone.Destination = this.Destination;
            return clone;
        }

        public ScpTcpPacket(byte[] buffer, int bufferlength=0)
        {
            Source = "";
            if (bufferlength==0)
            {
                bufferlength = buffer.Length;
            }
            int length = BitConverter.ToInt32(buffer, 0);
            if ((length + 10) != bufferlength)
            {
                throw new Exception("Invalid packet");
            }
            Broadcast = buffer[4] == 1;
            type = (ScpTcpPacketTypes)buffer[5];
            ID = BitConverter.ToInt32(buffer, 6);
            payload = new byte[bufferlength-10];
            Array.Copy(buffer, 10, payload,0, bufferlength-10);
        }

        public ScpTcpPacket(ScpTcpPacketTypes type, byte[] payload, bool broadcast)
        {
            Source = "";
            this.Broadcast = broadcast;
            if (payload == null)
            {
                this.payload = new byte[0];
            }
            else
            {
                this.payload = payload.ToArray();
            }
            this.type = type;
        }
        public byte[] GetBytes()
        {
            byte[] message = BitConverter.GetBytes(payload.Length);
            Array.Resize(ref message, 6);
            message[4] = (byte)((Broadcast) ? 1 : 0);
            message[5] = (byte)type;
            
            return message.Concat(BitConverter.GetBytes(ID)).Concat(payload).ToArray();
        }

    }
    /// <summary>
    /// Base class for Scp UDP packets. Main purpose is to construct correct Scp UDP packet from received bytes
    /// Used only internally by SCPHost class.
    /// </summary>
    public abstract class ScpUdpPacket
    {
        public static ScpUdpPacket Create(byte[] bytes)
        {
            if (bytes[0] == 1) // Master discover packet
            {
                return new ScpMasterDiscover(bytes);
            }
            else if (bytes[0] == 2) // Master discover reply packet
            {
                return new ScpMasterDiscoverReply(bytes);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Udp packet for discovering other masters on network
    /// </summary>
    /// 
    public class ScpMasterDiscover : ScpUdpPacket
    {
        public string FromHostName;
        public ScpMasterDiscover(byte[] bytes)
        {
            FromHostName = Encoding.ASCII.GetString(bytes, 1, bytes.Length - 1);
        }
        public ScpMasterDiscover(string FromHostName)
        {
            this.FromHostName = FromHostName;
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[1];
            bytes[0] = 1;
            return bytes.Concat(Encoding.ASCII.GetBytes(FromHostName)).ToArray();
        }
    }

    public class ScpMasterDiscoverReply : ScpUdpPacket
    {
        public string FromHostName;
        public IPEndPoint MasterIPEndPoint;
        public int MasterPriority;
        public ScpMasterDiscoverReply(byte[] bytes)
        {
            MasterPriority = bytes[1];
            FromHostName = Encoding.ASCII.GetString(bytes, 2, bytes.Length - 2);
        }
        public ScpMasterDiscoverReply()
        {
            this.FromHostName = ScpHost.Name;
            this.MasterPriority = ScpHost.Priority;
        }

        /// <summary>
        /// Gets the raw packet bytes for sending on the network
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[2];
            bytes[0] = 2;
            bytes[1] = (byte)MasterPriority;
            return bytes.Concat(Encoding.ASCII.GetBytes(FromHostName)).ToArray();
        }
    }
}
