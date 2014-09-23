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
        /// <summary>
        /// Used internally by ScpHost when in Master mode. Handles connections with slaves.
        /// </summary>
        private class ScpTcpServer
        {
            private bool enabled = false;
            private int clientsConnected = 0;
            private object _lock;

            private List<Task> connections = new List<Task>();
            private List<ScpTcpClient> scpClients = new List<ScpTcpClient>();
            private TcpListener tcpListener;

            public event MessageEventHandler MessageEvent;
            public event ScpInternalPacketEventHandler PacketEvent;
            public event SlaveConnectionEventHandler SlaveConnectionEvent;
            private void OnMessageEvent(MessageEventArgs e)
            {
                if (MessageEvent != null)
                {
                    MessageEvent(this, e);
                }
            }
            private void OnSlaveConnectionEvent(object sender, SlaveConnectionEventArgs e)
            {
                if (SlaveConnectionEvent != null)
                {
                    SlaveConnectionEvent(this, e);
                }
            }

            // Triggered when Master receives a message from a slave
            private void OnPacketEvent(object sender, ScpInternalPacketEventArgs e)
            {
                if (e.Packet.IsBroadcast()) // Broadcast the packet to other slaves
                {
                    Task broadcast = this.BroadcastAsync(e.Packet);
                    broadcast.Wait();
                }
                if (PacketEvent != null)
                {
                    PacketEvent(this, e);
                    /*if (e.Packet.Type == ScpTcpPacketTypes.Request && e.Response != null)
                    {
                        // Create the Response packet
                        ScpTcpPacket response = e.Response;
                        response.ID = e.Packet.ID;
                        BroadcastAsync(response);
                    }*/
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

            public async Task BroadcastAsync(ScpPacket packet)
            {
                if (packet.IsBroadcast())
                {
                    foreach (ScpTcpClient scpClient in scpClients.Where(scp => scp.Hostname != packet.Source))
                    {
                        await scpClient.SendAsync(packet);
                    }
                }
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
                    scpClient.PacketEvent -= scpClient_PacketEvent;
                    scpClient.PacketEvent += OnPacketEvent;
                    scpClients.Add(scpClient);
                    // Slave connected event
                    OnMessageEvent(new MessageEventArgs("Slave connected: " + scpClient.Hostname));
                    OnSlaveConnectionEvent(this, new SlaveConnectionEventArgs(true, scpClient.Hostname));
                    await scpClientTask;
                    // Slave disconnected event
                    OnSlaveConnectionEvent(this, new SlaveConnectionEventArgs(false, scpClient.Hostname));
                    OnMessageEvent(new MessageEventArgs("Slave disconnected: " + scpClient.Hostname));
                    scpClients.Remove(scpClient);
                }

                lock (_lock)
                {
                    clientsConnected--;
                }
            }

            private async void scpClient_PacketEvent(object sender, ScpInternalPacketEventArgs e)
            {
                if (e.Packet is ScpRegRequest)
                {
                    ScpRegRequest request = (ScpRegRequest)e.Packet;
                    byte[] test = request.GetBytes();
                    ScpPacket packet = new ScpRegResponse(true);
                    packet.Id = request.Id;
                    ((ScpTcpClient)sender).Hostname = request.Hostname;
                    try
                    {
                        await ((ScpTcpClient)sender).SendAsync(packet).ConfigureAwait(false);
                    }
                    catch // F.ex. timeout
                    {
                    }
                }
            }
        }
    }
}
