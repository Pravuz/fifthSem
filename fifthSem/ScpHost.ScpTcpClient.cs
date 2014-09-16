using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace ScadaCommunicationProtocol
{
    public partial class ScpHost
    {
        private class ScpTcpClient
        {
            public TcpClient tcpClient;
            public AutoResetEvent ClientDisconnectedEvent;
            public event MessageEventHandler MessageEvent;
            public event ScpPacketEventHandler PacketEvent;
            public Task ReaderTask;

            // Hostname to other end of connection
            public string Hostname { get; set; }

            private IPAddress address;
            private int port;
            private NetworkStream ns;
            private int pendingRequestCount;
            private int requestCounter = 0;
            private bool pendingRequest = false;
            private bool enabled = false;
            private CancellationTokenSource requestCancelToken = new CancellationTokenSource();
            private CancellationTokenSource clientCancelToken = new CancellationTokenSource();
            private ScpTcpPacket requestPacket;
            private ScpTcpPacket responsePacket;
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
                    if (e.Response != null)
                    {
                        Task send = SendAsync(e.Response);
                        send.Wait();
                    }
                }
            }

            public ScpTcpClient()
            {
                Hostname = "";
                ClientDisconnectedEvent = new AutoResetEvent(false);
            }

            /// <summary>
            /// Sends a packet asyncronously to the network.
            /// Throws an exception if response is not received in time or there are any network errors
            /// </summary>
            /// <param name="packet"></param>
            /// <returns>ScpTcpPacket returned. For Push requests, this return value is null</returns>
            public async Task<ScpTcpPacket> SendAsync(ScpTcpPacket packet)
            {
                if (!packet.IsResponse)
                {
                    packet.ID = requestCounter;
                }
                byte[] packetbuffer = packet.GetBytes();
                ScpTcpPacket response = null;

                // Send packet
                OnMessageEvent(new MessageEventArgs("SCP packet sent! ID: " + packet.ID.ToString() + " Type: " + packet.Type.ToString()));
                await ns.WriteAsync(packetbuffer, 0, packetbuffer.Length);
                if (packet.IsRequest) // Wait for response
                {
                    pendingRequestCount = requestCounter;
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
                requestCounter++;
                return response;
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
                        await Task.Delay(100);
                        Array.Copy(tempbuffer, 0, buffer, totalbytesread, bytesread);
                        totalbytesread += bytesread;
                        if (totalbytesread < 4)
                        {
                            continue;
                        }
                        if (packetLength == -1) // New packet starting
                        {
                            packetLength = BitConverter.ToInt32(buffer, 0);
                        }
                        if (totalbytesread >= (packetLength+10)) // Complete packet received
                        {
                            ScpTcpPacket packet = new ScpTcpPacket(buffer, packetLength + 10);
                            packet.Source = Hostname;
                            OnMessageEvent(new MessageEventArgs("SCP packet received! From: "+Hostname+" ID: " + packet.ID.ToString() + " Type: "+packet.Type.ToString()));
                            if (pendingRequest && packet.IsResponse)
                            {
                                responsePacket = packet;
                                requestCancelToken.Cancel(); // response to a pending request
                            }
                            else
                            {
                                ScpTcpPacket clone = packet.Clone();
                                OnPacketEvent(new ScpPacketEventArgs(clone));
                            }

                            if (totalbytesread>(packetLength+10))
                            {
                                Array.Copy(buffer, packetLength+10,buffer,0,totalbytesread-(packetLength+10));
                            }
                            totalbytesread -= packetLength + 10;
                            packetLength = -1;
                        }
                    }
                    tcpClient.Close();
                }
                catch
                {
                }
                finally
                {
                    ClientDisconnectedEvent.Set();
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
                await tcpClient.ConnectAsync(address, port);
                ReaderTask = ReaderAsync();
                try
                {
                    ScpTcpPacket request = new ScpTcpPacket(ScpTcpPacketTypes.RegRequest, Encoding.ASCII.GetBytes(hostname), false);
                    ScpTcpPacket response = await SendAsync(request);
                    if (response == null || response.Type != ScpTcpPacketTypes.RegResponse)
                    {
                        Disconnect();
                    }
                    else if (response.Payload[0] != 1) // Verify that Master acceps this slave connection
                    {
                        Disconnect();
                    }
                    else
                    {
                        connected = true;
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
