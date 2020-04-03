using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using NetworkChat.Properties;

namespace NetworkChat
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            textBox2.Text = Settings.Default.Email;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // Process.Start("http://NetworkChat.co.uk/buynetworkchat.php");
        }

        private void button2_Click(object sender, EventArgs e)
        {
          //  activate();
        }

        private void activate()
        {
            //conect to register.php
            string loc = "http://NetworkChat.co.uk/register/";
            // create a request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(loc);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            string post_data = "reg=" + textBox1.Text.Trim() + "&em=" + textBox2.Text.Trim();
            // turn our request string into a byte stream
            byte[] postBytes = Encoding.ASCII.GetBytes(post_data);

            // this is important - make sure you specify type this way
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
           // Console.WriteLine(response.StatusCode);
            response.Close();

            if (result.IndexOf("Thank You!") > -1)
            {
                //save to registry for current user
                Registry.SetUserAuthenticated(textBox2.Text.Trim(), textBox1.Text.Trim());
                //MessageBox.Show(
                this.Close();
            }
           // else
           // {
                MessageBox.Show(result, result, MessageBoxButtons.OK, MessageBoxIcon.Information);
           // }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.Email = textBox2.Text.Trim();
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
