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
    public enum ScpConnectionStatus { Stopped, Waiting, Master, Slave };

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    public delegate void ScpConnectionStatusEventHandler(object sender, ScpConnectionStatusEventArgs e);
    public delegate void PacketEventHandler(object sender, ScpPacketEventArgs e);
    public delegate void SlaveConnectionEventHandler(object sender, SlaveConnectionEventArgs e);
    public class SlaveConnectionEventArgs : EventArgs
    {
        public bool Connected;
        public string Name;
        public SlaveConnectionEventArgs(bool Connected, string Name)
        {
            this.Connected = Connected;
            this.Name = Name;
        }
    }

    public class ScpConnectionStatusEventArgs : EventArgs
    {
        public ScpConnectionStatus Status;
        public ScpConnectionStatusEventArgs(ScpConnectionStatus Status)
        {
            this.Status = Status;
        }
    }
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public MessageEventArgs(string Message)
        {
            this.Message = Message;
        }
    }

    /// <summary>
    /// This class establishes a SCP host, and automatically detect Master/Slave status of this host (ScpConnectionStatus).
    /// </summary>
    public partial class ScpHost
    {
        static public string Name;
        static public int Priority;
        static public readonly int UdpServerPort = 1234;
        static public readonly int TcpServerPort = 1234;

        /// <summary>
        /// Debug event.....
        /// </summary>
        public event MessageEventHandler MessageEvent;

        /// <summary>
        /// Event is triggered whenever a SCP packet is recieved (Either a SCP-Request or SCP-Push).
        /// Handler for this event should set the Response property of the EventArgs to reply to the request.
        /// </summary>
        public event PacketEventHandler PacketEvent;

        /// <summary>
        /// Triggered whenever there is a change in the connection status of this SCP host.
        /// </summary>
        public event ScpConnectionStatusEventHandler ScpConnectionStatusEvent;

        /// <summary>
        /// When in Master mode, triggered when a slave connects/disconnects.
        /// Includes hostname of slave, and if it was a connection or disconnection.
        /// </summary>
        public event SlaveConnectionEventHandler SlaveConnectionEvent;

        private ScpConnectionStatus scpConnectionStatus = ScpConnectionStatus.Stopped;


        private ScpUdpServer scpUdpServer;
        private ScpUdpClient scpUdpClient;

        private ScpTcpServer scpTcpServer;
        private ScpTcpClient scpTcpClient;

        private bool canBeMaster = true;
        private IPAddress masterIPAddress;
        private Task checkTask = null;
        private bool forceMaster = false;
        private CancellationTokenSource cancelMaster;

        public ScpConnectionStatus ScpConnectionStatus
        {
            get
            {
                return scpConnectionStatus;
            }
        }
        public List<string> Hosts
        {
            get
            {
                return scpTcpServer.Hosts;
            }
        }
        /// <summary>
        /// Used to control if this host can be SCP master or not.
        /// Default: true
        /// </summary>
        public bool CanBeMaster
        {
            get
            {
                return canBeMaster;
            }
            set
            {
                if (scpConnectionStatus == ScpConnectionStatus.Master && value == false)
                {
                    scpUdpServer.Stop();
                    scpTcpServer.Stop();
                    setConnectionStatus(ScpConnectionStatus.Waiting);
                }
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
            if (PacketEvent != null)
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
        private void OnSlaveConnectionEvent(object sender, SlaveConnectionEventArgs e)
        {
            if (SlaveConnectionEvent != null)
            {
                SlaveConnectionEvent(this, e);
            }
        }
        public ScpHost(int priority)
        {
            ScpHost.Priority = priority;
            ScpHost.Name = System.Environment.MachineName;

            cancelMaster = new CancellationTokenSource();

            scpUdpServer = new ScpUdpServer();
            scpUdpClient = new ScpUdpClient();
            scpTcpServer = new ScpTcpServer(this);
            scpTcpClient = new ScpTcpClient(this);

            scpTcpClient.MessageEvent += OnMessageEvent;
            scpTcpClient.PacketEvent += OnPacketEvent;

            scpTcpServer.MessageEvent += OnMessageEvent;
            scpTcpServer.PacketEvent += OnPacketEvent;
            scpTcpServer.SlaveConnectionEvent += OnSlaveConnectionEvent;
        }

        public async Task<bool> RequestSwitchToMaster()
        {
            if (scpConnectionStatus == ScpConnectionStatus.Slave)
            {
                ScpPacket response = await SendRequestAsync(new ScpMasterRequest());
                if ((response != null) && (response is ScpMasterResponse))
                {
                    if (((ScpMasterResponse)response).Ok)
                    {
                        forceMaster = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Sends a Broadcast packet asyncronously to all SCP hosts.
        /// Throws an exception if there are any network errors
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public async Task SendBroadcastAsync(ScpPacket packet)
        {
            //ScpPacket packet = new ScpPacket(ScpTcpPacketTypes.Broadcast, data);
            try
            {
                if (scpConnectionStatus == ScpConnectionStatus.Slave)
                {
                    await scpTcpClient.SendAsync(packet).ConfigureAwait(false);
                }
                else if (scpConnectionStatus == ScpConnectionStatus.Master)
                {
                    await scpTcpServer.BroadcastAsync(packet).ConfigureAwait(false);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sends a Request packet asyncronously to the SCP master.
        /// Throws an exception if response is not received in time or there are any network errors
        /// If used when in Master mode, returns null.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>ScpTcpPacket returned</returns>
        public async Task<ScpPacket> SendRequestAsync(ScpPacket request)
        {
            ScpPacket response = null;
            try
            {
                if (scpConnectionStatus == ScpConnectionStatus.Slave)
                {
                    //ScpTcpPacket packet = new ScpTcpPacket(ScpTcpPacketTypes.Request, data);
                    //ScpTcpPacket response = await scpTcpClient.SendAsync(packet).ConfigureAwait(false);
                    response = await scpTcpClient.SendAsync(request).ConfigureAwait(false);
                }
            }
            catch
            {
            }
            return response;
        }
        public void Start()
        {
            if (scpConnectionStatus == ScpConnectionStatus.Stopped)
            {
                setConnectionStatus(ScpConnectionStatus.Waiting);
                checkTask = Task.Run(() => checkScpConnection());
            }
        }

        public void Stop()
        {
            if (scpConnectionStatus != ScpConnectionStatus.Stopped)
            {
                if (scpConnectionStatus == ScpConnectionStatus.Master)
                {
                    setConnectionStatus(ScpConnectionStatus.Stopped);
                    cancelMaster.Cancel();
                }
                else if (scpConnectionStatus == ScpConnectionStatus.Slave)
                {
                    setConnectionStatus(ScpConnectionStatus.Stopped);
                    scpTcpClient.Disconnect();
                }
                checkTask.Wait();
            }
        }

        private void CancelMaster()
        {
            if (scpConnectionStatus == ScpConnectionStatus.Master)
            {
                setConnectionStatus(ScpConnectionStatus.Waiting);
                cancelMaster.Cancel();
            }
        }
        /// <summary>
        /// Checks whether a host is connected. Only available when running in Master mode.
        /// </summary>
        /// <param name="Hostname"></param>
        /// <returns>True if hostname is connected</returns>
        public bool IsHostConnected(string Hostname)
        {
            return scpTcpServer.IsHostConnected(Hostname);
        }

        /// <summary>
        /// Allowed hostnames must be added here.
        /// </summary>
        /// <param name="Hostname"></param>
        public void AddHost(string Hostname)
        {
            scpTcpServer.Hosts.Add(Hostname);
        }

        public void ClearHosts()
        {
            scpTcpServer.Hosts.Clear();
        }
        /// <summary>
        /// Detects master/slave role of this host on the network
        /// </summary>
        private async Task checkScpConnection()
        {
            ScpMasterDiscoverReply reply;
            int delay = 1000;
            try
            {
                while (scpConnectionStatus != ScpConnectionStatus.Stopped)
                {
                    OnMessageEvent(this, new MessageEventArgs("Trying to discover master...."));
                    if ((!forceMaster) && (scpUdpClient.DiscoverMaster(out reply)))
                    {
                        // Take slave role and connect to master
                        masterIPAddress = reply.MasterIPEndPoint.Address;
                        OnMessageEvent(this, new MessageEventArgs("Taking role as slave, connecting to master: " + reply.FromHostName + " (" + masterIPAddress.ToString() + ")"));
                        scpTcpClient.Hostname = reply.FromHostName;

                        // Open TCP connection to master
                        bool connected = await scpTcpClient.Connect(reply.MasterIPEndPoint.Address, ScpHost.TcpServerPort, ScpHost.Name);
                        if (connected)
                        {
                            delay = 1000;
                            setConnectionStatus(ScpConnectionStatus.Slave);
                            try
                            {
                                await scpTcpClient.ReaderTask.ConfigureAwait(false);
                            }
                            catch
                            {
                            }
                            OnMessageEvent(this, new MessageEventArgs("Connection to master lost."));
                            if (scpConnectionStatus != ScpConnectionStatus.Stopped)
                            {
                                setConnectionStatus(ScpConnectionStatus.Waiting);
                            }
                            scpTcpClient.Disconnect();
                            if (!forceMaster && scpConnectionStatus != ScpConnectionStatus.Stopped)
                            {
                                await Task.Delay(1000 + Priority * 1000);
                            }
                        }
                        else
                        {
                            OnMessageEvent(this, new MessageEventArgs("Failure connecting to master."));
                            await Task.Delay(delay);
                            delay *= 2;
                        }

                    }
                    else if (canBeMaster)
                    {
                        // Take master role. 
                        forceMaster = false;
                        OnMessageEvent(this, new MessageEventArgs("No master found, taking role as master."));
                        scpUdpServer.Start(); // Listen for UDP broadcast
                        scpTcpServer.Start(); // Start SCP server

                        setConnectionStatus(ScpConnectionStatus.Master);

                        // checking if other master is present, fall back to slave if another master with higher priority exists
                        while (scpConnectionStatus == ScpConnectionStatus.Master)
                        {
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
                            try
                            {
                                await Task.Delay(5000, cancelMaster.Token);
                            }
                            catch
                            {
                                OnMessageEvent(this, new MessageEventArgs("Cancel master request!"));
                                scpUdpServer.Stop();
                                scpTcpServer.Stop();
                                cancelMaster = new CancellationTokenSource();
                            }
                        }
                        if (scpConnectionStatus != ScpConnectionStatus.Stopped)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    else
                    {
                        await Task.Delay(1000);// Make sure we wait some seconds before trying another connection...
                    }
                }
            }
            catch
            {
                setConnectionStatus(ScpConnectionStatus.Stopped);
            }
        }

    }
}
