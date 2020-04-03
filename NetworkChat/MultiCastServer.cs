using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetworkChat
{
    class MultiCastServer
    {
        private static IPAddress mcastAddress;
        private static int mcastPort = 11111;
        private static Socket mcastSocket;
        private static MulticastOption mcastOption;


        public static void CancelMultiCast()
        {
            //mcastSocket.Shutdown(SocketShutdown.Both);
            //mcastSocket.Close();
        }

        public static void StartMulticast(IPAddress LocalIPAddress, IPAddress MultiCastIPAddress)
        {
            mcastAddress = MultiCastIPAddress;

            try
            {
                mcastSocket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);

              //  Console.Write("Enter the local IP address: ");

                IPAddress localIPAddr = LocalIPAddress;// IPAddress.Parse(Console.ReadLine());

                //IPAddress localIP = IPAddress.Any;
                EndPoint localEP = (EndPoint)new IPEndPoint(localIPAddr, mcastPort);

                mcastSocket.Bind(localEP);


                // Define a MulticastOption object specifying the multicast group 
                // address and the local IPAddress.
                // The multicast group address is the same as the address used by the server.
                mcastOption = new MulticastOption(mcastAddress, localIPAddr);

                mcastSocket.SetSocketOption(SocketOptionLevel.IP,
                                            SocketOptionName.AddMembership,
                                            mcastOption);

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static void ReceiveBroadcastMessages()
        {
            bool done = false;
            byte[] bytes = new Byte[4096];
            IPEndPoint groupEP = new IPEndPoint(mcastAddress, mcastPort);
            EndPoint remoteEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);


            try
            {
                while (!done)
                {
                    Console.WriteLine("Waiting for multicast packets.......");
                    Console.WriteLine("Enter ^C to terminate.");

                    mcastSocket.ReceiveFrom(bytes, ref remoteEP);

                    Console.WriteLine("Received broadcast from {0} :\n {1}\n",
                      groupEP.ToString(),
                      Encoding.ASCII.GetString(bytes, 0, bytes.Length));


                }


            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                mcastSocket.Close();
            }
        }





        public static List<IPAddress> getMulticastIPs()
        {
            List<IPAddress> mips = new List<IPAddress>();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                if (nic.SupportsMulticast)
                {
                    IPInterfaceProperties ipp = nic.GetIPProperties();
                    foreach (MulticastIPAddressInformation mip in ipp.MulticastAddresses)
                    {
                        if (mip.Address != null)
                        {
                            if (mip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                if (!mips.Contains(mip.Address))
                                    mips.Add(mip.Address);
                            }
                        }
                    }
                }
            }

            return mips;
        }


    }
}
