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
    public enum ScpConnectionStatus { Waiting, Master, Slave };

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    public delegate void ScpPacketEventHandler(object sender, ScpPacketEventArgs e);
    public delegate void ScpConnectionStatusEventHandler(object sender, ScpConnectionStatusEventArgs e);

    public class ScpConnectionStatusEventArgs : EventArgs
    {
        public ScpConnectionStatus Status;
        public ScpConnectionStatusEventArgs(ScpConnectionStatus Status)
        {
            this.Status = Status;
        }
    }
    public class ScpPacketEventArgs : EventArgs
    {
        public ScpTcpPacket Packet;
        public ScpTcpPacket Response;
        public ScpPacketEventArgs(ScpTcpPacket Packet)
        {
            this.Packet = Packet;
            Response = null;
        }
    }
    // Class for the data to be passed to message event handlers
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public MessageEventArgs(string Message)
        {
            this.Message = Message;
        }
    }

    public partial class ScpHost
    {
        static public string Name;
        static public int Priority;
        static public readonly int UdpServerPort = 1234;
        static public readonly int TcpServerPort = 1236;

        public event MessageEventHandler MessageEvent;

        /// <summary>
        /// Event is triggered whenever a SCP packet is recieved (Either a SCP-Request or SCP-Push).
        /// Handler for this event should set the Response property of the EventArgs to reply to the request.
        /// </summary>
        public event ScpPacketEventHandler PacketEvent;

        /// <summary>
        /// Triggered whenever there is a change in the connection status of this SCP host.
        /// </summary>
        public event ScpConnectionStatusEventHandler ScpConnectionStatusEvent;

        private ScpConnectionStatus scpConnectionStatus = ScpConnectionStatus.Waiting;


        private ScpUdpServer scpUdpServer;
        private ScpUdpClient scpUdpClient;

        private ScpTcpServer scpTcpServer;
        private ScpTcpClient scpTcpClient;

        private bool canBeMaster = true;
        private IPAddress masterIPAddress;
        public bool CanBeMaster
        {
            get
            {
                return canBeMaster;
            }
            set
            {
                canBeMaster = value;
            }
        }

        private void setConnectionStatus(ScpConnectionStatus status)
        {
            if (status == scpConnectionStatus)
            {
                return; // No change so just return
            }
            scpConnectionStatus = status;
            if (status == ScpConnectionStatus.Slave)
            {
                scpUdpServer.Stop();
                scpTcpServer.Stop();
            }
            else if (status == ScpConnectionStatus.Waiting)
            {

                scpUdpServer.Stop();
                scpTcpServer.Stop();
            }
            OnScpConnectionStatusEvent(this, new ScpConnectionStatusEventArgs(status));
        }

        private void OnMessageEvent(object sender, MessageEventArgs e)
        {
            if (MessageEvent != null)
            {
                MessageEvent(this, e);
            }
        }
        private void OnPacketEvent(object sender, ScpPacketEventArgs e)
        {
            if (PacketEvent != null) // Don't raise the event in case it has already been handled by ScpTcpClient/Server
            {
                PacketEvent(this, e);
            }
        }
        private void OnScpConnectionStatusEvent(object sender, ScpConnectionStatusEventArgs e)
        {
            if (ScpConnectionStatusEvent != null)
            {
                ScpConnectionStatusEvent(this, e);
            }
        }
        public ScpHost(int priority)
        {
            ScpHost.Priority = priority;
            ScpHost.Name = System.Environment.MachineName;

            scpUdpServer = new ScpUdpServer();
            scpUdpClient = new ScpUdpClient();
            scpTcpServer = new ScpTcpServer();
            scpTcpClient = new ScpTcpClient();

            scpTcpServer.MessageEvent += OnMessageEvent;
            scpTcpClient.MessageEvent += OnMessageEvent;

            scpTcpClient.PacketEvent += OnPacketEvent;
            scpTcpServer.PacketEvent += OnPacketEvent;

        }

        /// <summary>
        /// Sends a packet asyncronously to the network.
        /// Throws an exception if response is not received in time or there are any network errors
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>ScpTcpPacket returned. For Push requests, this return value is null</returns>
        public async Task<ScpTcpPacket> SendPacket(ScpTcpPacket packet)
        {
            ScpTcpPacket response = null;
            if (scpConnectionStatus == ScpConnectionStatus.Slave)
            {
                response = await scpTcpClient.SendAsync(new ScpTcpPacket(ScpTcpPacketTypes.KeepAlive, null, true));
            }
            else if (scpConnectionStatus == ScpConnectionStatus.Master)
            {
                response = await scpTcpServer.SendAsync(new ScpTcpPacket(ScpTcpPacketTypes.KeepAlive, null, true));
            }
            return response;
        }
        public void SendPacket()
        {
            if (scpConnectionStatus == ScpConnectionStatus.Slave)
            {
                Task task = scpTcpClient.SendAsync(new ScpTcpPacket(ScpTcpPacketTypes.KeepAlive, null, true));
            }
            else if (scpConnectionStatus == ScpConnectionStatus.Master)
            {
                Task task = scpTcpServer.SendAsync(new ScpTcpPacket(ScpTcpPacketTypes.KeepAlive, null, true));
            }
        }

        public void Start()
        {
            Task checkTask = checkScpConnection();
        }
        /// <summary>
        /// Detects master/slave role of this host on the network
        /// </summary>
        private async Task checkScpConnection()
        {
            ScpMasterDiscoverReply reply;
            while (true)
            {
                OnMessageEvent(this, new MessageEventArgs("Trying to discover master...."));
                if (scpUdpClient.DiscoverMaster(out reply))
                {
                    // Take slave role and connect to master
                    masterIPAddress = reply.MasterIPEndPoint.Address;
                    OnMessageEvent(this, new MessageEventArgs("Taking role as slave, connecting to master: " + reply.FromHostName + " (" + masterIPAddress.ToString() + ")"));
                    scpTcpClient.Hostname = reply.FromHostName;

                    // Open TCP connection to master
                    bool connected = await scpTcpClient.Connect(reply.MasterIPEndPoint.Address, ScpHost.TcpServerPort, ScpHost.Name);
                    if (connected)
                    {
                        setConnectionStatus(ScpConnectionStatus.Slave);
                        await scpTcpClient.ReaderTask;
                        OnMessageEvent(this, new MessageEventArgs("Connection to master lost."));
                        setConnectionStatus(ScpConnectionStatus.Waiting);
                        scpTcpClient.Disconnect();
                    }
                    else
                    {
                        OnMessageEvent(this, new MessageEventArgs("Failure connecting to master."));
                    }

                }
                else if (canBeMaster)
                {
                    // Take master role. 
                    OnMessageEvent(this, new MessageEventArgs("No master found, taking role as master."));
                    scpUdpServer.Start(); // Listen for UDP broadcast
                    scpTcpServer.Start(); // Start SCP server

                    setConnectionStatus(ScpConnectionStatus.Master);

                    // checking if other master is present, fall back to slave if another master with higher priority exists
                    while (scpConnectionStatus == ScpConnectionStatus.Master)
                    {
                        await Task.Delay(5000);
                        if (scpUdpClient.DiscoverMaster(out reply))
                        {
                            if ((reply.MasterPriority <= ScpHost.Priority) && (reply.FromHostName != ScpHost.Name))
                            {
                                OnMessageEvent(this, new MessageEventArgs("Other higher priority master found: " + reply.FromHostName + " switching to slave mode"));
                                setConnectionStatus(ScpConnectionStatus.Waiting);
                                scpUdpServer.Stop();
                                scpTcpServer.Stop();
                            }
                        }
                    }
                }
                else
                {
                    await Task.Delay(5000);// Make sure we wait some seconds before trying another connection...
                }
                //await Task.Delay(1000);// Make sure we wait some seconds before trying another connection...
            }
        }

    }
}
