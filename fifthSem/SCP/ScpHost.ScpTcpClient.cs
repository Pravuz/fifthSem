using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Concurrent;

namespace ScadaCommunicationProtocol
{
    public partial class ScpHost
    {
        /// <summary>
        ///  Private class used to keep track of any pending requests sent while waiting for the response
        /// </summary>
        private class PendingRequest
        {
            public int ID { get; set; }
            public ScpPacket Response { get; set; }
            public CancellationTokenSource RequestCancellationToken { get; set; }
            public PendingRequest(int ID)
            {
                this.RequestCancellationToken = new CancellationTokenSource();
                this.ID = ID;
                Response = null;
            }
        }
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
            private List<PendingRequest> pendingRequests;
            private bool enabled = false;
            public CancellationTokenSource requestCancelToken = new CancellationTokenSource();
            private CancellationTokenSource cancelClient = new CancellationTokenSource();
            private Object _lock = new Object();
            private ScpHost scpHost;
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
                        if ((e.Response is ScpMasterResponse) && (((ScpMasterResponse)e.Response).Ok)) 
                        {
                            // In this case the master has agreed to let another master take over
                            scpHost.CancelMaster();
                        }
                    }
                }
            }
            public ScpTcpClient(ScpHost scpHost)
            {
                this.scpHost = scpHost;
                Hostname = "";
                pendingRequests = new List<PendingRequest>();
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

                if (!packet.ToString().Contains("Temp"))
                {
                    OnMessageEvent(new MessageEventArgs("SCP packet sent to: " + Hostname + "! ID: " + packet.Id.ToString() + " Type: " + packet.ToString()));
                }
                if (packet.IsRequest())
                {
                    PendingRequest pendingRequest = new PendingRequest(packet.Id);
                    lock (pendingRequests)
                    {
                        pendingRequests.Add(pendingRequest);
                    }
                    WritePacket(packetbuffer);
                    try
                    {
                        // Wait for up to 5 seconds for the response.
                        // When/if response is received (in ReaderAsync()), this delay is cancelled using the cancellationtoken.
                        await Task.Delay(5000, pendingRequest.RequestCancellationToken.Token);
                    }
                    catch
                    {
                    }
                    lock (pendingRequests)
                    {
                        pendingRequests.Remove(pendingRequest);
                    }
                    if (pendingRequest.Response != null)
                    {
                        response = pendingRequest.Response;
                    }
                    else
                    {
                        throw new Exception("Timeout!");
                    }
                }
                else
                {
                    WritePacket(packetbuffer);
                }
                return response;
            }

            private void WritePacket(byte[] data)
            {
                lock (ns)
                {
                    int offset = 0;
                    while (offset < (data.Length - 8192))
                    {
                        ns.Write(data, offset, 8192);
                        offset += 8192;
                    }
                    int count = data.Length - offset;
                    ns.Write(data, 0, count);
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
                        bytesread = await ns.ReadAsync(tempbuffer, 0, 8192, cancelClient.Token);
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
                                        if (!packet.ToString().Contains("Temp"))
                                        {
                                            OnMessageEvent(new MessageEventArgs("SCP packet received! From: " + Hostname + " ID: " + packet.Id.ToString() + " Type: " + packet.ToString()));
                                        }
                                        if (packet.IsResponse())
                                        {
                                            PendingRequest pendingRequest = null;
                                            lock (pendingRequests)
                                            {
                                                pendingRequest = pendingRequests.FirstOrDefault(req => req.ID == packet.Id);
                                            }
                                            if (pendingRequest != null)
                                            {
                                                pendingRequest.Response = packet;
                                                pendingRequest.RequestCancellationToken.Cancel();
                                            }
                                        }
                                        else if (!packet.IsResponse())
                                        {
                                            ScpPacket clone = packet.Clone();
                                            OnPacketEvent(new ScpPacketEventArgs(clone));
                                        }

                                    }
                                    else if (packetLength != 4)
                                    {
                                        OnMessageEvent(new MessageEventArgs("Invalid SCP packet received! From: " + Hostname + " ID: " + packetLength.ToString()));
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
                try
                {
                    while (enabled)
                    {
                        WritePacket(keepAlivepacket);
                        await Task.Delay(1000, cancelClient.Token);
                    }
                }
                catch
                {
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
                    ReaderTask = ReaderAsync();
                    ScpPacket request = new ScpRegRequest(hostname);
                    ScpPacket response = await SendAsync(request);
                    if (response != null && response is ScpRegResponse && ((ScpRegResponse)response).Ok)
                    {
                        connected = true;
                        keepAliveTask = KeepAlive();
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
                //writerTask = WriterAsync();
                keepAliveTask = KeepAlive();
                ReaderTask = ReaderAsync();
                await ReaderTask;
            }

            public void Disconnect()
            {
                try
                {
                    enabled = false;
                    cancelClient.Cancel();
                    tcpClient.Client.Close();
                    tcpClient.Close();
                    Task.WaitAll(ReaderTask);
                    cancelClient = new CancellationTokenSource();
                }
                finally
                {
                }
            }
        }
    }
}
