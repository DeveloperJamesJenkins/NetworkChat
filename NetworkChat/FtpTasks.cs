using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NetworkChat.Properties;
using System.Net.Sockets;
using System.IO;

namespace NetworkChat
{
    class FtpTasks
    {
        public static void UploadData(string Uri, string Contents)
        {
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(Uri);
            //req.CachePolicy
        }

        /// <summary>
        /// creates or opens the task file 
        /// </summary>
        public static void CreateTaskFile()
        {

            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Tasks></Tasks>";
            xml = XmlEncryption.encryptXml(xml);
            string Uri = "ftp://" + Settings.Default.FTPHost + @"/Tasks.xml.txt";

            NetworkCredential nc = new NetworkCredential();
            nc.UserName = Settings.Default.FTPUser;
            nc.Password = Settings.Default.FTPPassword;

            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(Uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = nc;

            MemoryStream ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml));

            StreamReader sourceStream = new StreamReader(ms);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
            

        }
    }
}
