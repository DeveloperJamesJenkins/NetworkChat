using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Xml;
using System.Drawing;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using NetworkChat.Properties;

namespace NetworkChat
{
    class Chat
    {
        public delegate void d_NewMessage(Lan.NetworkUser Computer, string Message);
        public static event d_NewMessage NewMessage;
        public delegate void d_NewFile(string FromID, string From, string FileName);
        public static event d_NewFile NewFile;
        public delegate void d_FileSent(int FilesCount);
        public static event d_FileSent FileSent;

        public delegate void d_MessageSent();
        public event d_MessageSent MessageSent;

        public delegate void d_UserOffline(NetworkChat.Lan.NetworkUser User);
        public event d_UserOffline UserOffline;

        private static Socket s_chatListener;
        private static Socket s_fileListener;

        private void userOffline(NetworkChat.Lan.NetworkUser User)
        {
            if (UserOffline != null)
                UserOffline(User);
        }


        public static void StopListener()
        {
            //s_chatListener.Shutdown(SocketShutdown.Both);
            try
            {
                s_chatListener.Close();
                //s_fileListener.Shutdown(SocketShutdown.Both);
                s_fileListener.Close();
            }
            catch (Exception ex)
            {

            }
        }


        //new listener for file uploads,,,
        public static void ListenForFile(IPAddress LocalIPAddress)
        {
            try
            {
                IPEndPoint ipend = new IPEndPoint(LocalIPAddress, (int)Settings.Default.FilePort);
                s_fileListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s_fileListener.Bind(ipend);
                s_fileListener.Listen(10);

          
                while (true)
                {

                    Socket handler = s_fileListener.Accept();
                   
                    ParameterizedThreadStart pst = new ParameterizedThreadStart(saveFile);
                    Thread t = new Thread(pst);
                    t.Start(handler);
                    
                  
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        static void saveFile(object res)
        {
            Socket handler = (Socket)res;
            NetworkStream ns = new NetworkStream(handler);
            XmlSerializer xss = new XmlSerializer(typeof(string));
            object o = xss.Deserialize(ns);
            ns.Close();

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(o.ToString());
            //now get file from xml
            string name = xdoc.SelectSingleNode("FileContainer/File").Attributes["name"].InnerText;
            string b64 = xdoc.SelectSingleNode("FileContainer/File").InnerText;
            string from = xdoc.SelectSingleNode("FileContainer/From").InnerText;
            string fromname = xdoc.SelectSingleNode("FileContainer/FromName").InnerText;

             bool todo = true;

            if (!Settings.Default.AcceptTransfers)
            {
                //find sender...

                if (MessageBox.Show("Accept File Transfer from " + fromname + "?", "Accept File Transfer " + fromname + "?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    todo = false;
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();


                }
            }

            if (todo)
            {
                byte[] fileBytes = Convert.FromBase64String(b64);

                string path = Settings.Default.TransfersFolder;
                if (path == "")
                {
                    string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (!Directory.Exists(docs + @"\NetworkChat"))
                        Directory.CreateDirectory(docs + @"\NetworkChat");

                    if (!Directory.Exists(docs + @"\NetworkChat\files"))
                        Directory.CreateDirectory(docs + @"\NetworkChat\files");


                    if (!Directory.Exists(docs + @"\files\" + from))
                        Directory.CreateDirectory(docs + @"\NetworkChat\files\" + from);

                    path = docs + @"\NetworkChat\files";//\" + from + @"\" + name; //dont use userUniqueID

                   // Application.StartupPath
                }
                else
                {
                 //   path = path;

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    if (!Directory.Exists(path + @"\" + from))
                        Directory.CreateDirectory(path + @"\" + from);
                }

                File.WriteAllBytes(path + @"\" + from + @"\" + name, fileBytes);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();



                if (NewFile != null)
                    NewFile(from, fromname, name);

                GC.Collect();
            }
        }

        public static void Listen(IPAddress LocalIPAddress)
        {
            try
            {
                IPEndPoint ipend = new IPEndPoint(LocalIPAddress, (int)Settings.Default.ChatPort);
                s_chatListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s_chatListener.Bind(ipend);
                s_chatListener.Listen(10);

            
                while (true)
                {
                    Socket handler = s_chatListener.Accept();

                    ParameterizedThreadStart pst = new ParameterizedThreadStart(loadMessageFromSocket);
                    Thread t = new Thread(pst);
                    t.Start(handler);
                    
                   
                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

        }

        private static void loadMessageFromSocket(object socket)
        {
            Socket handler = (Socket)socket;
            string data = "";
            while (true)
            {
                byte[] butes = new byte[1024];
                int rec = handler.Receive(butes);

                data += ASCIIEncoding.ASCII.GetString(butes);

                if (data.IndexOf("\0") > -1)
                    break;
            }


            //string r = handler.RemoteEndPoint.ToString();
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();


            returnNewMessage(data);
        }

        private static void returnNewMessage(string envelope)
        {
            envelope = XmlEncryption.decryptXml( envelope);

            XmlDocument x = new XmlDocument();
            x.LoadXml(envelope);
            XmlNode user = x.SelectSingleNode("envelope/user");
            XmlNode computer = x.SelectSingleNode("envelope/computer");
            XmlNode ip = x.SelectSingleNode("envelope/ip");
            string message = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String( x.SelectSingleNode("envelope/message").InnerText));
            XmlNode presence = x.SelectSingleNode("envelope/presence");
            XmlNode dateSent = x.SelectSingleNode("envelope/timestamp");

            Lan.NetworkUser nc = new Lan.NetworkUser();
            nc.UserName = user.InnerText;
            nc.ComputerName = computer.InnerText;
            nc.IPAddress = IPAddress.Parse( ip.InnerText );
            nc.Presence = (Lan.Presence)Enum.Parse(typeof(Lan.Presence), presence.InnerText);
            nc.TimeStamp = DateTime.Parse(dateSent.InnerText);
            nc.UserUniqueId = (nc.UserName + nc.ComputerName).ToLower();

            if (NewMessage != null)
                NewMessage(nc, message);
            
        }


        public void SendMessage(Lan.NetworkUser User, string Message, string[] Files)
        {

            try
            {
                string toSend = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>";
                toSend += "<envelope><user>" + Environment.UserName + "</user><computer>" + Environment.MachineName + "</computer>";

                toSend += "<ip>" + User.IPAddress + "</ip>";
                toSend += "<presence>" + Enum.GetName(typeof(Lan.Presence), User.Presence) + "</presence>";
                toSend += "<timestamp>" + DateTime.Now.ToString() + "</timestamp>";
                toSend += "<message>" + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Message)) + "</message>";
                toSend += "</envelope>";
                string enc_toSend = XmlEncryption.encryptXml(toSend);

                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s.SendTimeout = (int) Settings.Default.Timeout;
                s.Connect(User.IPAddress, (int)Settings.Default.ChatPort);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(enc_toSend);
                int i = s.Send(bytesToSend);
                s.Close();

                
            //send files
                if (Files.Length > 0)
                {
                    foreach (string file in Files)
                    {
                        Thread.Sleep(1000);
                        sendFile(file, User);

                        if (FileSent != null)
                            FileSent(Files.Length);
                    }

                }

                if (MessageSent != null)
                    MessageSent();

            }
            catch (Exception ex)
            {

                //fire the event alertin to a lost user User...
               // userOffline(User);
            }

        }

        private void sendFile(string file, Lan.NetworkUser User)
        {
            lock (this)
            {
                try
                {
                    FileInfo fi = new FileInfo(file);
                    string name = fi.Name;

                    string contents = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>";
                    contents += "<FileContainer>";
                    contents += "<FromName>" + Environment.UserName + "</FromName>";
                    contents += "<From>" + Environment.UserName + Environment.MachineName + "</From>";
                    contents += "<File name=\"" + name + "\">";
                    byte[] bits = File.ReadAllBytes(file);
                    contents += Convert.ToBase64String(bits);
                    contents += "</File></FileContainer>";

                    XmlSerializer xss = new XmlSerializer(typeof(string));
                    Socket sFile = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    
                    sFile.SendTimeout = (int)Settings.Default.Timeout;
                    sFile.Connect(User.IPAddress, (int)Settings.Default.FilePort);
                    NetworkStream ns = new NetworkStream(sFile);
                    xss.Serialize(ns, contents);
                    ns.Close();
                    sFile.Shutdown(SocketShutdown.Both);
                    sFile.Close();

                    GC.Collect();

                   


                }
                catch (Exception ex)
                {

                }
            }
        }

      
    }
}
