using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;

namespace ScadaCommunicationProtocol
{
    public delegate void ScpInternalPacketEventHandler(object sender, ScpPacketEventArgs e);
    public class ScpPacketEventArgs : EventArgs
    {
        public ScpPacket Packet;
        public ScpPacket Response;
        public ScpPacketEventArgs(ScpPacket Packet)
        {
            this.Packet = Packet;
            Response = null;
        }
    }
    public abstract class ScpPacket
    {
        protected enum ScpPacketTypes { RegRequest = 1, RegResponse = 51, LogFileRequest = 2, LogFileResponse = 52, AlarmRequest = 3, AlarmResponse = 53, TempBroadcast = 100, AlarmBroadcast = 101, AlarmLimitBroadcast = 102 };
        private static int newId = 0;
        public static int GetId()
        {
            return Interlocked.Increment(ref newId);
        }
        protected int id;
        protected int type;
        public string Source { get; set; }
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public bool IsBroadcast()
        {
            return type >= 100;
        }
        
        public bool IsRequest()
        {
            return type < 50;
        }

        public bool IsResponse()
        {
            return (type >= 50) & (type < 100);
        }
        public ScpPacket()
        {
            id = GetId();
        }
        public ScpPacket(byte[] bytes)
        {
            type = bytes[4];
            id = BitConverter.ToInt32(bytes, 6);
        }

        public static ScpPacket Create(byte[] bytes, int length=0)
        {
            ScpPacket packet = null;
            if (length == 0)
            {
                length = bytes.Length;
            }
            if (length >= 10)
            {
                try
                {
                    switch ((ScpPacketTypes)bytes[4])
                    {
                        case ScpPacketTypes.RegRequest:
                            packet = new ScpRegRequest(bytes, length);
                            break;
                        case ScpPacketTypes.RegResponse:
                            packet = new ScpRegResponse(bytes, length);
                            break;
                        case ScpPacketTypes.AlarmBroadcast:
                            packet = new ScpAlarmBroadcast(bytes, length);
                            break;
                        case ScpPacketTypes.AlarmLimitBroadcast:
                            packet = new ScpAlarmLimitBroadcast(bytes, length);
                            break;
                        case ScpPacketTypes.LogFileRequest:
                            packet = new ScpLogFileRequest(bytes, length);
                            break;
                        case ScpPacketTypes.LogFileResponse:
                            packet = new ScpLogFileResponse(bytes, length);
                            break;
                        case ScpPacketTypes.TempBroadcast:
                            packet = new ScpTempBroadcast(bytes, length);
                            break;
                        case ScpPacketTypes.AlarmRequest:
                            packet = new ScpAlarmRequest(bytes, length);
                            break;
                        case ScpPacketTypes.AlarmResponse:
                            packet = new ScpAlarmResponse(bytes, length);
                            break;
                    }
                }
                catch
                {

                }
            }
            return packet;
        }
        public byte[] GetBytes()
        {
            return GetBytes(GetPayload());
        }
        protected byte[] GetBytes(byte[] payload)
        {
            byte[] message = BitConverter.GetBytes(payload.Length + 10);
            Array.Resize(ref message, 6);
            message[4] = (byte)type;
            message[5] = (byte)0;

            return message.Concat(BitConverter.GetBytes(id)).Concat(payload).ToArray();
        }
        
        protected abstract byte[] GetPayload();
        public ScpPacket Clone()
        {
            ScpPacket clone = ScpPacket.Create(GetBytes());
            clone.Source = this.Source;
            return clone;
        }
    }
    public class ScpRegRequest : ScpPacket
    {
        private string hostname;
        public string Hostname
        {
            get
            {
                return hostname;
            }
        }
        public ScpRegRequest(string hostname) : base()
        {
            this.hostname = hostname;
            type = (byte)ScpPacketTypes.RegRequest;
        }
        public ScpRegRequest(byte[] bytes, int length)
            : base(bytes)
        {
            hostname = Encoding.ASCII.GetString(bytes, 10, length - 10);
        }
        protected override byte[] GetPayload()
        {
            return Encoding.ASCII.GetBytes(hostname);
        }
        public override string ToString()
        {
            return "ScpRegRequest - Hostname: " + hostname;
        }
    }
    public class ScpRegResponse : ScpPacket
    {
        private bool ok;
        public bool Ok
        {
            get
            {
                return ok;
            }
        }
        public ScpRegResponse(bool ok)
        {
            this.ok = ok;
            type = (byte)ScpPacketTypes.RegResponse;
        }
        public ScpRegResponse(byte[] bytes, int length)
            : base(bytes)
        {
            ok = bytes[10] == 1;
        }
        protected override byte[] GetPayload()
        {
            byte[] payload = new byte[1];
            payload[0] = (byte)(ok ? 1 : 0);
            return payload;
        }
        public override string ToString()
        {
            return "ScpRegResponse - OK: " + ok.ToString();
        }
    }
    public class ScpLogFileRequest : ScpPacket
    {
        private long fileSize;
        public long FileSize
        {
            get
            {
                return fileSize;
            }
        }
        public ScpLogFileRequest(long fileSize) : base()
        {
            this.fileSize = fileSize;
            type = (byte)ScpPacketTypes.LogFileRequest;
        }
        public ScpLogFileRequest(byte[] bytes, int length)
            : base(bytes)
        {
            fileSize = BitConverter.ToInt32(bytes, 10);
        }
        protected override byte[] GetPayload()
        {
            return BitConverter.GetBytes(fileSize);
        }
        public override string ToString()
        {
            return "ScpLogFileRequest - Filesize: " + fileSize.ToString();
        }
    }
    public class ScpLogFileResponse : ScpPacket
    {
        private byte[] file;
        public byte[] File
        {
            get
            {
                return file;
            }
        }
        public ScpLogFileResponse(byte[] file)
        {
            if (file == null)
            {
                this.file = new byte[0];
            }
            else
            {
                this.file = file;
            }
            type = (byte)ScpPacketTypes.LogFileResponse;
        }
        public ScpLogFileResponse(byte[] bytes, int length)
            : base(bytes)
        {
            file = new byte[length - 10];
            Array.Copy(bytes, 10, file, 0, length - 10);
        }
        protected override byte[] GetPayload()
        {
            return file;
        }
        public override string ToString()
        {
            if (file == null || file.Length == 0)
            {
                return "ScpLogFileResponse - No file needed";
            }
            else
            {
                return "ScpLogFileResponse - Master filesize: " + file.Length.ToString();
            }
        }
    }

    public class ScpAlarmRequest : ScpPacket
    {
        private fifthSem.AlarmTypes alarmType;
        private fifthSem.AlarmCommand alarmCommand;
        private string alarmSource;
        public fifthSem.AlarmCommand AlarmCommand
        {
            get
            {
                return alarmCommand;
            }
        }
        public fifthSem.AlarmTypes AlarmType
        {
            get
            {
                return alarmType;
            }
        }
        public string AlarmSource
        {
            get
            {
                return alarmSource;
            }
        }
        public ScpAlarmRequest(fifthSem.AlarmTypes Type, fifthSem.AlarmCommand Command, string source)
            : base()
        {
            this.alarmType = Type;
            this.alarmCommand = Command;
            this.alarmSource = source;
            type = (byte)ScpPacketTypes.AlarmRequest;
        }
        public ScpAlarmRequest(byte[] bytes, int length)
            : base(bytes)
        {
            alarmCommand = (fifthSem.AlarmCommand)bytes[10];
            alarmType = (fifthSem.AlarmTypes)bytes[11];
            alarmSource = Encoding.ASCII.GetString(bytes, 12, length - 12);
        }
        protected override byte[] GetPayload()
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)alarmCommand;
            bytes[1] = (byte)alarmType;
            return bytes.Concat(Encoding.ASCII.GetBytes(alarmSource)).ToArray();
        }
        public override string ToString()
        {
            return "ScpAlarmRequest - Command: " + alarmCommand.ToString() + " Alarmtype: " + alarmType.ToString();
        }
    }
    public class ScpAlarmResponse : ScpPacket
    {
        private bool ok;
        public bool Ok
        {
            get
            {
                return ok;
            }
        }
        public ScpAlarmResponse(bool ok)
        {
            this.ok = ok;
            type = (byte)ScpPacketTypes.AlarmResponse;
        }
        public ScpAlarmResponse(byte[] bytes, int length)
            : base(bytes)
        {
            ok = (bytes[10] == 1);
        }
        protected override byte[] GetPayload()
        {
            byte[] bytes = new byte[1];
            bytes[0] = (ok) ? (byte)1 : (byte)0;
            return bytes;
        }
        public override string ToString()
        {
            return "ScpAlarmResponse - " + (ok ? "OK" : "Not OK");
        }
    }


    public class ScpTempBroadcast : ScpPacket
    {
        private double temp;
        public double Temp
        {
            get
            {
                return temp;
            }
        }
        public ScpTempBroadcast(double temp)
            : base()
        {
            this.temp = temp;
            type = (byte)ScpPacketTypes.TempBroadcast;
        }
        public ScpTempBroadcast(byte[] bytes, int length)
            : base(bytes)
        {
            temp = BitConverter.ToDouble(bytes, 10);
        }
        protected override byte[] GetPayload()
        {
            return BitConverter.GetBytes(temp);
        }
        public override string ToString()
        {
            return "ScpTempBroadcast - Temp: " + temp.ToString();
        }
    }
    public class ScpAlarmBroadcast : ScpPacket
    {
        private byte[] alarm;
        public byte[] Alarm
        {
            get
            {
                return alarm;
            }
        }
        public ScpAlarmBroadcast(byte[] alarm)
            : base()
        {
            this.alarm = alarm;
            type = (byte)ScpPacketTypes.AlarmBroadcast;
        }
        public ScpAlarmBroadcast(byte[] bytes, int length)
            : base(bytes)
        {
            alarm = new byte[length - 10];
            Array.Copy(bytes, 10, alarm, 0, length - 10);
        }
        protected override byte[] GetPayload()
        {
            return alarm;
        }
        public override string ToString()
        {
            return "ScpAlarmBroadcast";
        }
    }
    public class ScpAlarmLimitBroadcast : ScpPacket
    {
        private double loLoLimit;
        private double loLimit;
        private double hiLimit;
        private double hiHiLimit;
        public double LoLoLimit { get { return loLoLimit; } }
        public double LoLimit { get { return loLimit; } }
        public double HiLimit { get { return hiLimit; } }
        public double HiHiLimit { get { return hiHiLimit; } }
        public ScpAlarmLimitBroadcast(double loLoLimit, double loLimit, double hiLimit, double hiHiLimit)
            : base()
        {
            this.loLoLimit = loLoLimit;
            this.loLimit = loLimit;
            this.hiLimit = hiLimit;
            this.hiHiLimit = hiHiLimit;
            type = (byte)ScpPacketTypes.AlarmLimitBroadcast;
        }
        public ScpAlarmLimitBroadcast(byte[] bytes, int length)
            : base(bytes)
        {
            loLoLimit = BitConverter.ToDouble(bytes, 10);
            loLimit = BitConverter.ToDouble(bytes, 18);
            hiLimit = BitConverter.ToDouble(bytes, 26);
            hiHiLimit = BitConverter.ToDouble(bytes, 34);
        }
        protected override byte[] GetPayload()
        {
            byte[] payload = new byte[8 * 4];
            Array.Copy(BitConverter.GetBytes(loLoLimit), 0, payload, 0, 8);
            Array.Copy(BitConverter.GetBytes(loLimit), 0, payload, 8, 8);
            Array.Copy(BitConverter.GetBytes(hiLimit), 0, payload, 16, 8);
            Array.Copy(BitConverter.GetBytes(hiHiLimit), 0, payload, 24, 8);

            return payload;
        }
        public override string ToString()
        {
            return "ScpAlarmLimitBroadcast - LoLo: " + loLoLimit.ToString()+
                   " Lo: " + loLimit.ToString()+
                   " Hi: " + hiLimit.ToString()+
                   " HiHi: " + hiHiLimit.ToString();
        }
    }

    public partial class ScpHost
    {
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
        private class ScpMasterDiscover : ScpUdpPacket
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

        private class ScpMasterDiscoverReply : ScpUdpPacket
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
}
