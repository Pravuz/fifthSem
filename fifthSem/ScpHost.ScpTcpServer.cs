using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Net;

namespace ScadaCommunicationProtocol
{
    public partial class ScpHost
    {
        private class ScpTcpServer
        {
            private bool enabled = false;
            private int clientsConnected = 0;
            private object _lock;

            private List<Task> connections = new List<Task>();
            private List<ScpTcpClient> scpClients = new List<ScpTcpClient>();
            private TcpListener tcpListener;

            public event MessageEventHandler MessageEvent;
            public event ScpPacketEventHandler PacketEvent;
            private void OnMessageEvent(MessageEventArgs e)
            {
                if (MessageEvent != null)
                {
                    MessageEvent(this, e);
                }
            }
            private void OnPacketEvent(object sender, ScpPacketEventArgs e)
            {
                if (e.Packet.Broadcast) // Broadcast the packet to other slaves
                {
                    Task broadcast = this.SendAsync(e.Packet);
                    broadcast.Wait();
                }
                if (PacketEvent != null)
                {
                    PacketEvent(this, e);
                }
            }
            public ScpTcpServer()
            {
                _lock = new object();
            }

            public void Start()
            {
                if (!enabled)
                {
                    enabled = true;
                    Task listenerTask = listener();
                }
            }

            public void Stop()
            {
                if (enabled)
                {
                    tcpListener.Stop();
                    // Disconnect all slaves
                    foreach (ScpTcpClient scpTcpClient in scpClients)
                    {
                        scpTcpClient.Disconnect();
                    }
                    scpClients.Clear();
                    enabled = false;
                }
            }

            public async Task<ScpTcpPacket> SendAsync(ScpTcpPacket packet)
            {
                ScpTcpPacket response = null;
                if (packet.Broadcast)
                {
                    foreach (ScpTcpClient scpClient in scpClients.Where(scp => scp.Hostname != packet.Source))
                    {
                        await scpClient.SendAsync(packet);
                    }
                }
                else
                {
                    ScpTcpClient scpClient = scpClients.SingleOrDefault(scp => scp.Hostname == packet.Destination);
                    if (scpClient != null)
                    {
                        response = await scpClient.SendAsync(packet);
                    }
                }
                return response;
            }

            private async Task listener()
            {
                tcpListener = new TcpListener(IPAddress.Any, ScpHost.TcpServerPort);
                tcpListener.Start();
                while (enabled)
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();
                    Task connector = connectClientAsync(client);
                }
            }

            private async Task connectClientAsync(TcpClient client)
            {
                lock (_lock)
                {
                    clientsConnected++;
                }
                ScpTcpClient scpClient = new ScpTcpClient();
                scpClient.PacketEvent += scpClient_PacketEvent;
                scpClient.MessageEvent += MessageEvent;
                Task scpClientTask = scpClient.Connect(client);

                await Task.Delay(1000);
                if (scpClient.Hostname == "") // No response from master, so disconnect
                {
                    scpClient.Disconnect();
                }
                else
                {
                    OnMessageEvent(new MessageEventArgs("Slave connected: " + scpClient.Hostname));
                    scpClients.Add(scpClient);
                }
                scpClient.PacketEvent -= scpClient_PacketEvent;
                scpClient.PacketEvent += OnPacketEvent;
                // Wait for registration from client
                await scpClientTask;
                OnMessageEvent(new MessageEventArgs("Slave disconnected: " + scpClient.Hostname));

                scpClients.Remove(scpClient);
                lock (_lock)
                {
                    clientsConnected--;
                }
            }

            private void scpClient_PacketEvent(object sender, ScpPacketEventArgs e)
            {
                if (e.Packet.Type == ScpTcpPacketTypes.RegRequest)
                {
                    byte[] test = e.Packet.GetBytes();
                    //OnMessageEvent(new MessageEventArgs("Reg request received."+BitConverter.ToString(e.Packet.GetBytes())));
                    ScpTcpPacket packet = new ScpTcpPacket(ScpTcpPacketTypes.RegResponse, new byte[] { 1 }, false);
                    packet.ID = e.Packet.ID;
                    ((ScpTcpClient)sender).Hostname = Encoding.ASCII.GetString(e.Packet.Payload);
                    ((ScpTcpClient)sender).SendAsync(packet);
                    TcpClient tcpClient = ((ScpTcpClient)sender).tcpClient;
                }
            }

            private void tcpTimeout(Object source, ElapsedEventArgs e)
            {
                // Disconnect the client
                OnMessageEvent(new MessageEventArgs("Timeout!"));
            }

            private async Task clientAsync(TcpClient tcpClient)
            {
                byte[] message = new byte[8192];
                int bytesRead;
                System.Timers.Timer timeout = new System.Timers.Timer(2000);//Timer((TimerCallback)tcpTimeout,tcpClient,5000,2000);
                timeout.Elapsed += tcpTimeout;


                NetworkStream ns = tcpClient.GetStream();

                timeout.Start();
                while (enabled && tcpClient.Connected)
                {
                    try
                    {
                        while (true)
                        {
                            // Implement TCP protocol messages
                            bytesRead = await ns.ReadAsync(message, 0, 8192);
                            timeout.Interval = 2000;

                        }
                    }
                    catch
                    {
                    }
                    break;
                }
                OnMessageEvent(new MessageEventArgs("Client disconnected, ip address: " + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()));
                tcpClient.Close();

            }
        }
    }
}
