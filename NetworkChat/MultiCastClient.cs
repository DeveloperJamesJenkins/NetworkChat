using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace NetworkChat
{
    class MultiCastClient
    {
        private static IPAddress mcastAddress;
        private static int mcastPort = 11111;
        private static Socket mcastSocket;
        private static MulticastOption mcastOption;

        public static void JoinMulticastGroup(IPAddress LocalIPAddress, IPAddress MultiCastIPAddress)
        {
            //try
            //{
            // Create a multicast socket.

            mcastAddress = MultiCastIPAddress;
            mcastSocket = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Dgram,
                                     ProtocolType.Udp);

            // Get the local IP address used by the listener and the sender to
            // exchange multicast messages. 
            //Console.Write("\nEnter local IPAddress for sending multicast packets: ");
            IPAddress localIPAddr = LocalIPAddress;// IPAddress.Parse(Console.ReadLine());

            // Create an IPEndPoint object. 
            IPEndPoint IPlocal = new IPEndPoint(localIPAddr, 0);

            // Bind this endpoint to the multicast socket.
            mcastSocket.Bind(IPlocal);

            // Define a MulticastOption object specifying the multicast group 
            // address and the local IP address.
            // The multicast group address is the same as the address used by the listener.
            MulticastOption mcastOption;
            mcastOption = new MulticastOption(mcastAddress, localIPAddr);

            mcastSocket.SetSocketOption(SocketOptionLevel.IP,
                                        SocketOptionName.AddMembership,
                                        mcastOption);

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\n" + e.ToString());
            //}
        }

        public static void BroadCastPresence(NetworkChat.Lan.Presence Presence)
        {
            IPEndPoint endPoint;

            NetworkChat.Lan.NetworkUser lc = NetworkChat.Lan.GetLocalComputer();
            string presence = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>";
            presence += "<envelope><user>" + Environment.UserName;
            presence += "</user><computer>" + lc.ComputerName + "</computer>";
            //Image Oimg = Image.FromFile(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\" + Environment.UserName + ".bmp");
            //Image Nimg = new Bitmap(Oimg, new Size(32, 32));

            //Image img = Nimg; // Image.FromFile(@"C:\Users\James\Downloads\mouse_silence.gif");//
            //MemoryStream ms = new MemoryStream();
            //img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            //byte[] buffer = ms.ToArray();
            //ms.Close();

            ////FileStream fs = new FileStream(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\" + Environment.UserName + ".bmp", FileMode.Open);
            ////byte[] buffer = new byte[fs.Length];
            ////fs.Read(buffer, 0, buffer.Length);
            ////fs.Close();

            ////ms.Write(buffer, 0, buffer.Length);
            //string ico = Convert.ToBase64String(buffer);
            //presence += "<icon>" + ico + "</icon>";
            presence += "<timestamp>" + DateTime.Now + "</timestamp>";
            presence += "<ip>" + lc.IPAddress + "</ip>";
            presence += "<presence>" + Enum.GetName(typeof(NetworkChat.Lan.Presence), Presence) + "</presence>";
            presence += "</envelope>";

            string enc_presence = XmlEncryption.encryptXml(presence);

            //try
            //{
            //Send multicast packets to the listener.
            Socket mcastSocket = new Socket(AddressFamily.InterNetwork,
                                        SocketType.Dgram,
                                        ProtocolType.Udp);
            endPoint = new IPEndPoint(mcastAddress, mcastPort);
            mcastSocket.SendTo(Encoding.ASCII.GetBytes(enc_presence), endPoint);
           // Console.WriteLine("Multicast data sent.....");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\n" + e.ToString());
            //}

            mcastSocket.Close();
        }
    }
}
