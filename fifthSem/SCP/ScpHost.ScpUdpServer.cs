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
        /// <summary>
        /// Used internally by ScpHost when in Master mode. Purpose is to reply to UDP broadcasts from oth SCP hosts with master information.
        /// </summary>
        private class ScpUdpServer
        {
            private UdpClient udpMasterServer;
            private bool enabled = false;
            private Task receiveTask;

            public void Start()
            {
                if (!enabled)
                {
                    enabled = true;
                    udpMasterServer = new UdpClient(ScpHost.UdpServerPort);
                    // Start listening for UDP packets
                    receiveTask = ReceiveAsync();
                }
            }
            public void Stop()
            {
                if (enabled)
                {
                    enabled = false;
                    udpMasterServer.Close();
                    //receiveTask.Wait(1000);
                }
            }

            private async Task ReceiveAsync()
            {
                try
                {
                    while (true)
                    {
                        UdpReceiveResult result = await udpMasterServer.ReceiveAsync();
                        if (result.Buffer.Length > 0)
                        {
                            ScpUdpPacket packet = ScpUdpPacket.Create(result.Buffer);
                            if (packet != null && packet is ScpMasterDiscover)
                            {
                                ScpMasterDiscoverReply reply = new ScpMasterDiscoverReply();
                                byte[] bytes = reply.GetBytes();
                                await udpMasterServer.SendAsync(bytes, bytes.Length, result.RemoteEndPoint);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
