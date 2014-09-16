using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net;

namespace ScadaCommunicationProtocol
{
    public partial class ScpHost
    {
        /// <summary>
        /// This class handles discovering SCP master hosts using UDP broadcasts
        /// </summary>
        private class ScpUdpClient
        {
            private UdpClient udpClient;
            private List<IPAddress> broadcastAddresses;
            public ScpUdpClient()
            {
                udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;
                udpClient.Client.ReceiveTimeout = 100;
                broadcastAddresses = new List<IPAddress>();
                
                //findBroadcastAddresses();
                // Hardcoded broadcast address for testing.....
                broadcastAddresses.Add(new IPAddress(new byte[] {192,168,9,255}));
            }

            private void findBroadcastAddresses()
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;
                    if (networkInterface.OperationalStatus != OperationalStatus.Up)
                        continue;
                    IPInterfaceProperties ips = networkInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation ip in ips.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;
                        byte[] broadcastAddress = ip.Address.GetAddressBytes();
                        byte[] netmask = ip.IPv4Mask.GetAddressBytes();
                        for (int i = 0; i < broadcastAddress.Length; i++)
                        {
                            // Calculate broadcast address of this network
                            broadcastAddress[i] |= (byte)~netmask[i];
                        }
                        broadcastAddresses.Add(new IPAddress(broadcastAddress));
                    }
                }
            }
            public bool DiscoverMaster(out ScpMasterDiscoverReply reply)
            {
                ScpMasterDiscover discoverPacket = new ScpMasterDiscover(ScpHost.Name);
                byte[] bytes = discoverPacket.GetBytes();
                System.Net.IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

                foreach (IPAddress broadcastAddress in broadcastAddresses.ToList())
                {
                    try
                    {
                        udpClient.Send(bytes, bytes.Length, new IPEndPoint(broadcastAddress, ScpHost.UdpServerPort));
                        bytes = udpClient.Receive(ref ipEndPoint);
                        reply = new ScpMasterDiscoverReply(bytes);
                        reply.MasterIPEndPoint = ipEndPoint;

                        // Keep using only this broadcast address for subsequent broadcasts
                        broadcastAddresses.Clear();
                        broadcastAddresses.Add(broadcastAddress);
                        return true;
                    }
                    catch
                    {
                    };
                }
                reply = null;
                return false;
            }
        }
    }
}
