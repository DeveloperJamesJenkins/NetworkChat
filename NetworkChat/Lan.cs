using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.Drawing;
using NetworkChat.Properties;

namespace NetworkChat
{
    /// <summary>
    /// Does all the chat stuff.
    /// </summary>
    public class Lan
    {
        public static bool ShuttingDown { get; set; }
        private static UdpClient listener;
        //private static Socket slistener;
        private static IPEndPoint groupEP;
        

        public delegate void dNetworkUser(NetworkUser NetworkUser);
        public static event dNetworkUser NetworkUserPulse;
        private static void onNetworkUser(NetworkUser NetworkUser)
        {
            if (NetworkUserPulse != null)
                NetworkUserPulse(NetworkUser);
        }

        public static void ShutDown()
        {
            try
            {
                if (listener != null)
                {
                    if (listener.Client != null)
                    {
                        listener.Client.Shutdown(SocketShutdown.Both);
                      //  listener.Client.Disconnect(true);
                    }

                    //listener.Close();
                }
            }
            catch 
            {
            }
        }


        public static void BroadcastPresence(Presence Presence)
        {

            try
            {

                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                s.EnableBroadcast = true;

                IPAddress broadcast = IPAddress.Parse(Settings.Default.BroadcastAddress);// IPAddress.Broadcast;

                NetworkUser lc = GetLocalComputer();
                string presence = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>";
                presence += "<envelope><user>" + Environment.UserName;
                presence += "</user><computer>" + lc.ComputerName + "</computer>";
                presence += "<timestamp>" + DateTime.Now + "</timestamp>";
                presence += "<ip>" + lc.IPAddress + "</ip>";
                presence += "<presence>" + Enum.GetName(typeof(Presence), Presence) + "</presence>";
                presence += "</envelope>";

                string enc_presence = XmlEncryption.encryptXml(presence);



                byte[] sendbuf = Encoding.ASCII.GetBytes(enc_presence);
                IPEndPoint ep = new IPEndPoint(broadcast, (int)Settings.Default.BroadcastPort);

                s.SendTo(sendbuf, ep);
                s.Close();
            }
            catch
            {
            }
            
        }

        
        private static void RecieveBroadcast(IAsyncResult ar)
        {
            try
            {

                byte[] bits = listener.EndReceive(ar, ref groupEP);
                if (bits != null)
                {
                    string xml = ASCIIEncoding.ASCII.GetString(bits);
                    onNetworkUser(parseNetworkUser(xml));
                }
            }
            catch
            {

            }
           
        }

        
        public static void ListenForBroadcast(IPAddress IP, int Port)
            {
                try
                {
                    groupEP = new IPEndPoint(IP, Port); //new IPEndPoint(IPAddress.Any, Port);
                    //if (listener != null)
                    //    listener.Close();

                    listener = new UdpClient((int)Settings.Default.BroadcastPort);
                

                    while (!ShuttingDown)
                    {

                        Thread.Sleep(50);

                        if (listener.Client != null)
                        {
                            if (listener.Available > 0)
                            {
                                //  listener.ExclusiveAddressUse = false;

                                listener.BeginReceive(new AsyncCallback(RecieveBroadcast), null);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {

                   // MessageBox.Show(ex.Message);
                }
                finally
                {
                    //if (listener.Client != null)
                    //{
                    //    if(listener.Client.Connected)
                    //        listener.Client.Shutdown(SocketShutdown.Both);
                    //}

                    if(listener != null)
                        listener.Close();
                }
            }


        private static NetworkUser parseNetworkUser(string xml)
        {
            xml = XmlEncryption.decryptXml(xml);
            
            XmlDocument x = new XmlDocument();
            x.LoadXml(xml);
            XmlNode user = x.SelectSingleNode("envelope/user");
            XmlNode computer = x.SelectSingleNode("envelope/computer");
            XmlNode ip = x.SelectSingleNode("envelope/ip");
            XmlNode message = x.SelectSingleNode("envelope/message");
            XmlNode presence = x.SelectSingleNode("envelope/presence");
            XmlNode icon = x.SelectSingleNode("envelope/icon");
            XmlNode timestamp = x.SelectSingleNode("envelope/timestamp");

            //byte[] imgBits = Convert.FromBase64String(icon.InnerText);
            //MemoryStream ms = new MemoryStream(imgBits);
            //Image img = Image.FromStream(ms);
            //ms.Close();

            Lan.NetworkUser nc = new Lan.NetworkUser();
            nc.UserName = user.InnerText;
            nc.ComputerName = computer.InnerText;
            nc.IPAddress = IPAddress.Parse(ip.InnerText);
            nc.Presence = (Presence)Enum.Parse(typeof(Presence), presence.InnerText);
            nc.TimeStamp = DateTime.Parse(timestamp.InnerText);
            nc.UserUniqueId = (nc.UserName + nc.ComputerName);

           // nc.Icon = img;
            return nc;
        }

    


        public static NetworkUser GetLocalComputer()
        {
            NetworkUser nc = new NetworkUser();
            nc.ComputerName = Dns.GetHostName();

           // IPAddress[] ips = Dns.GetHostAddresses(nc.ComputerName);
           
                //Ping ping = new Ping();
                //PingReply pr = ping.Send(nc.ComputerName);

                //if (pr.Status == IPStatus.Success)
                //{
                   
                    IPHostEntry ipit = Dns.GetHostEntry(nc.ComputerName);
                    foreach (IPAddress ip in ipit.AddressList)
                    {

                        Match match = Regex.Match(ip.ToString(), @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
                        if (match.Success)
                        {
                            nc.IPAddress = IPAddress.Parse(match.Captures[0].Value);
                        }
                  //  }


                }

                return nc;

        }


        public static bool IsUserThere(NetworkUser NetworkUser)
        {
            try
            {
                Ping p = new Ping();

                PingReply pr = p.Send(NetworkUser.IPAddress);
                if (pr.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch { return false; }
            
           
        }
     

        public class NetworkUser
        {
            public string UserName { get; set; }
            public string UserUniqueId { get; set; }
            public string ComputerName { get; set; }
            public IPAddress IPAddress { get; set; }
            public Presence Presence { get; set; }
            public Image Icon { get; set; }
            public DateTime TimeStamp { get; set; }


            public override string ToString()
            {
                return "     " + UserName + " (" + Enum.GetName(typeof(Presence), Presence) + ")"; 
                
            }
        }


        public enum Presence
        {
            Online,
            Away,
            Busy,
            Offline,
        }

        
    }
}
