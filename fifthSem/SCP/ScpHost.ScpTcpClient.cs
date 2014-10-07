using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Net;

namespace ScadaCommunicationProtocol
{
    public partial class ScpHost
    {
        /// <summary>
        /// Used internally by ScpHost. Handles TCP sessions and communication.
        /// </summary>
        private class ScpTcpClient
        {
            public TcpClient tcpClient;
            public event MessageEventHandler MessageEvent;
            public event ScpInternalPacketEventHandler PacketEvent;
            public Task ReaderTask;
            private Task keepAliveTask;

            // Hostname to other end of connection
            public string Hostname { get; set; }

            private IPAddress address;
            private int port;
            private NetworkStream ns;
            private int pendingRequestID;
            private bool pendingRequest = false;
            private bool enabled = false;
            private CancellationTokenSource requestCancelToken = new CancellationTokenSource();
            private CancellationTokenSource clientCancelToken = new CancellationTokenSource();
            private ScpPacket requestPacket;
            private ScpPacket responsePacket;
            private Object _lock = new Object();
            private BufferBlock<byte[]> writeBuffer;
            private Task writerTask;
            private void OnMessageEvent(MessageEventArgs e)
            {
                if (MessageEvent != null)
                {
                    MessageEvent(this, e);
                }
            }
            private void OnPacketEvent(ScpPacketEventArgs e)
            {
                if (PacketEvent != null)
                {
                    PacketEvent(this, e);
                    if (e.Response != null) // Reply in case this was a request
                    {
                        e.Response.Id = e.Packet.Id;
                        SendAsync(e.Response).Wait();
                    }
                }
            }
            public ScpTcpClient()
            {
                Hostname = "";
            }
            /// <summary>
            /// Sends a packet asyncronously to the network.
            /// Throws an exception if response is not received in time or there are any network errors
            /// </summary>
            /// <param name="packet"></param>
            /// <returns>ScpTcpPacket returned. For Broadcast and Response packet, return value is null</returns>
            public async Task<ScpPacket> SendAsync(ScpPacket packet)
            {
                byte[] packetbuffer = packet.GetBytes();
                ScpPacket response = null;

                // Send packet
                OnMessageEvent(new MessageEventArgs("SCP packet sent! ID: " + packet.Id.ToString() + " Type: " + packet.ToString()));

                await writeBuffer.SendAsync(packetbuffer);
                if (packet.IsRequest())
                {
                    pendingRequestID = packet.Id;
                    pendingRequest = true;
                    requestPacket = packet;
                    responsePacket = null;
                    try
                    {
                        await Task.Delay(1000, requestCancelToken.Token);
                    }
                    catch
                    {

                    }
                    if (responsePacket != null)
                    {
                        response = responsePacket;
                    }
                    else
                    {
                        throw new Exception("Timeout!");
                    }
                    pendingRequest = false;
                }
                return response;
            }

            /// <summary>
            /// Waits for new packets to be added to outgoing packetqueue, then sends them
            /// </summary>
            /// <returns></returns>
            private async Task WriterAsync()
            {
                writeBuffer = new BufferBlock<byte[]>();
                while (enabled)
                {
                    byte[] data = await writeBuffer.ReceiveAsync();
                    int offset = 0;
                    while (offset < (data.Length-8192))
                    {
                        await ns.WriteAsync(data, offset, 8192);
                        offset += 8192;
                    }
                    int count = data.Length - offset;
                    await ns.WriteAsync(data, 0, count);
                }
            }

            /// <summary>
            /// Listens for incoming TCP traffic, and constructs SCP packets of the data.
            /// Triggers OnPacketEvent when valid Request packet is received.
            /// Response packets are returned by the SendAsync method.
            /// </summary>
            /// <returns></returns>
            private async Task ReaderAsync()
            {
                byte[] tempbuffer = new byte[8192];
                byte[] buffer = new byte[65536*16]; // Max size of packet payload - 1mbyte
                byte[] count = new byte[1];
                int bytesread;
                int totalbytesread=0;
                int packetLength = -1;
                count[0] = 0;
                try
                {
                    ns = tcpClient.GetStream();
                    while (enabled)
                    {
                        bytesread = await ns.ReadAsync(tempbuffer, 0, 8192);
                        Array.Copy(tempbuffer, 0, buffer, totalbytesread, bytesread);
                        totalbytesread += bytesread;
                        if (totalbytesread >= 4)
                        {
                            while (totalbytesread >= 4)
                            {
                                if (packetLength == -1) // New packet starting
                                {
                                    packetLength = BitConverter.ToInt32(buffer, 0);
                                }
                                if (totalbytesread >= (packetLength)) // Complete packet received
                                {
                                    ScpPacket packet = ScpPacket.Create(buffer,packetLength);//new ScpTcpPacket(buffer, packetLength);
                                    if (packet != null)
                                    {
                                        packet.Source = Hostname;
                                        OnMessageEvent(new MessageEventArgs("SCP packet received! From: " + Hostname + " ID: " + packet.Id.ToString() + " Type: " + packet.ToString()));
                                        if (pendingRequest && packet.IsResponse() && pendingRequestID == packet.Id)
                                        {
                                            responsePacket = packet;
                                            requestCancelToken.Cancel(); // Response is recevied, stop the SendAsync method from waiting any longer
                                            requestCancelToken = new CancellationTokenSource();
                                        }
                                        else if (!packet.IsResponse())
                                        {
                                            ScpPacket clone = packet.Clone();
                                            OnPacketEvent(new ScpPacketEventArgs(clone));
                                        }

                                    }
                                    // If bytes in buffer was more than one complete packet, move remaining bytes to beginning of buffer
                                    if (totalbytesread > (packetLength))
                                    {
                                        Array.Copy(buffer, packetLength, buffer, 0, totalbytesread - (packetLength));
                                    }
                                    totalbytesread -= packetLength;
                                    packetLength = -1;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    tcpClient.Close();
                }
                catch
                {
                }
            }

            private async Task KeepAlive()
            {
                byte[] keepAlivepacket = BitConverter.GetBytes(4);
                while (enabled)
                {
                    await writeBuffer.SendAsync(keepAlivepacket);
                    await Task.Delay(1000);
                }
            }

            // Used by slave for connecting to master
            public async Task<bool> Connect(IPAddress address, int port, string hostname)
            {
                bool connected = false;
                enabled = true;
                this.address = address;
                this.port = port;
                tcpClient = new TcpClient();
                try
                {
                    await tcpClient.ConnectAsync(address, port);
                    writerTask = WriterAsync();
                    ReaderTask = ReaderAsync();
                    ScpPacket request = new ScpRegRequest(hostname);
                    ScpPacket response = await SendAsync(request);
                    if (response != null && response is ScpRegResponse && ((ScpRegResponse)response).Ok)
                    {
                        connected = true;
                        keepAliveTask = Task.Run(() => KeepAlive());
                    }
                    else if (response != null && response is ScpRegResponse && !((ScpRegResponse)response).Ok)
                    {
                        OnMessageEvent(new MessageEventArgs("Not allowed to connect to Master!"));
                        Disconnect();
                    }
                    else
                    {
                        OnMessageEvent(new MessageEventArgs("RegResponse: " + (response == null ? "NULL" : "NOT NULL")));
                        Disconnect();
                    }
                }
                catch
                {
                    Disconnect();
                }
                return connected;
            }

            // Used by master for connecting to slave
            public async Task Connect(TcpClient client)
            {
                enabled = true;
                tcpClient = client;
                writerTask = WriterAsync();
                keepAliveTask = Task.Run(() => KeepAlive());
                await ReaderAsync();
            }

            public void Disconnect()
            {
                try
                {
                    enabled = false;
                    tcpClient.Close();
                }
                finally
                {
                }
            }
        }
    }
}
