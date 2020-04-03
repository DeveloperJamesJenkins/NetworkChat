using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetworkChat.Properties;
using System.Reflection;
using System.IO;
using System.Net;
using System.Threading;

namespace NetworkChat
{
    public partial class MainSettings : Form
    {
        public delegate void dDeletingMessages();
        public static event dDeletingMessages MessagesDeleted;
        bool networkChanges;

        public MainSettings()
        {
            InitializeComponent();

            loadSettings();
        }

        private void textBoxTaskFile_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void MainSettings_Load(object sender, EventArgs e)
        {

           
            
        }

        private void loadSettings()
        {
           // Registry.StartWithWindows(Settings.Default.StartWithWindows);

            string path;
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (Settings.Default.LogsFolder != "")
                textBoxLogs.Text = Settings.Default.LogsFolder;
            else
            {
                
                path = docs + @"\NetworkChat\messages";
                textBoxLogs.Text = path;// Application.StartupPath + @"\messages";
            }

            checkBoxAcceptTransfers.Checked = Settings.Default.AcceptTransfers;
            checkBoxEnableLogging.Checked = Settings.Default.EnableLogging;
           // textBoxLogs.Text = Settings.Default.LogsFolder;
            textBoxTaskFile.Text = Settings.Default.TaskFolder;
            numericUpDown1.Value = Settings.Default.LargestFileSizeMB;
            textBoxSoundLocation.Text = Settings.Default.SoundFileLocation;
            buttonDefaultFont.Font = Settings.Default.DefaultFont;
            buttonForeColor.BackColor = Settings.Default.DefaultFontColor;
            buttonBackColor.BackColor = Settings.Default.DefaultBackColor;
            textBoxTransferFolder.Text = Settings.Default.TransfersFolder;

           // textBoxEmail.Text = Settings.Default.Email;
            textBoxIP.Text = Lan.GetLocalComputer().IPAddress.ToString();

            numericUpDownBroadcastPort.Value = Settings.Default.BroadcastPort;
            numericUpDownChatPort.Value = Settings.Default.ChatPort;
            numericUpDownFilePort.Value = Settings.Default.FilePort;
            numericUpDownTimeout.Value = Settings.Default.Timeout;
            textBoxBroadcastAddress.Text = Settings.Default.BroadcastAddress;

            radioButtonUseShare.Checked = Settings.Default.UseLocalTasksNotFtp;
            if (radioButtonUseShare.Checked)
            {
                textBoxFTPHost.Enabled = false;
                numericUpDownPort.Enabled = false;
                textBoxFTPUser.Enabled = false;
                textBoxFtpPassword.Enabled = false;
            }
            else
            {
                textBoxTaskFile.Enabled = false;
                buttonSelectShare.Enabled = false;
            }

            radioButtonuseFtp.Checked = !Settings.Default.UseLocalTasksNotFtp;

            path = Settings.Default.TransfersFolder;
            if (path == "")
            {
                docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = docs + @"\NetworkChat\files\";
                
            }

            textBoxTransferFolder.Text = path;


            numericUpDownPort.Value = Settings.Default.FTPPort;
            textBoxFTPUser.Text = Settings.Default.FTPUser;
            textBoxFTPHost.Text = Settings.Default.FTPHost;
            textBoxFtpPassword.Text = Settings.Default.FTPPassword;

            loadGeneralTab();
            loadAlertsTab();
        }

        private void loadAlertsTab()
        {
            for (int i = 0; i < checkedListBoxAlerts.Items.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.PopupOnNewMessage);
                        break;
                    case 1:
                         checkedListBoxAlerts.SetItemChecked(i, Settings.Default.SoundOnNewMessage);
                        break;
                    case 2:
                        checkedListBoxAlerts.SetItemChecked(i,Settings.Default.SpeakMessageIsFrom);
                        break;
                    case 3:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.SpeakMessage);
                        break;
                    case 4:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.AlwaysSpeakMessages);
                        break;
                    case 5:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.PopupOnNewFileArrival);
                        break;
                    case 6:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.SoundOnNewFileArrival);
                        break;
                    case 7:
                        checkedListBoxAlerts.SetItemChecked(i,Settings.Default.SpeakOnNewFileArrival);
                        break;

                    case 8:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.SpeakNameWhenUserComesOnline);
                        break;
                    case 9:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.SoundWhenUserComesOnline);
                        break;
                    case 10:
                        checkedListBoxAlerts.SetItemChecked(i, Settings.Default.PopupWhenUserComesOnline);
                        break;
                }
            }
        }

        private void loadGeneralTab()
        {
           
            if (Settings.Default.StartWithWindows)
                checkBox1.Checked = true;
           
            if (Settings.Default.StartMinimized)
                checkBox2.Checked = true;
           
            if (Settings.Default.AwayStatusOnScreenSaver)
                checkBox3.Checked = true;
          
            if (!Settings.Default.ShowInTaskbar)
                checkBox4.Checked = true;
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string s = GetNetworkFolders(folderBrowserDialog1);

            if (s != "")
            {
                textBoxTaskFile.Text = s;

                Settings.Default.TaskFolder = textBoxTaskFile.Text.Trim();
                saveSettings();

                TasksLocalShare.CreateTaskFile(Settings.Default.TaskFolder);
            }
        }


        private string GetNetworkFolders(FolderBrowserDialog oFolderBrowserDialog)
        {
            Type type = oFolderBrowserDialog.GetType();
            FieldInfo fieldInfo = type.GetField("rootFolder", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(oFolderBrowserDialog, 18);
            
            if((textBoxTaskFile.Text.Trim() != "") & (Directory.Exists(textBoxTaskFile.Text.Trim())))
                oFolderBrowserDialog.SelectedPath = textBoxTaskFile.Text.Trim();

            if (oFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                return oFolderBrowserDialog.SelectedPath.ToString();
            }
            else
            {
                return "";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int val = (int)numericUpDown1.Value;
            Settings.Default.LargestFileSizeMB = val;
            saveSettings();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete All User Messages?", "Delete All User Messages?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                if(MessagesDeleted != null)
                    MessagesDeleted();

                UserMessages.DeleteAllMessages();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string oldPath = Settings.Default.LogsFolder;
            folderBrowserDialog1.SelectedPath = oldPath;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxLogs.Text = folderBrowserDialog1.SelectedPath;

                Settings.Default.LogsFolder = folderBrowserDialog1.SelectedPath;
                saveSettings();

                UserMessages.MoveFiles(oldPath);

            }
        }


        public static void saveSettings()
        {
            Settings.Default.Save();
            Settings.Default.Reload();
        }

        private void checkBoxAcceptTransfers_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.AcceptTransfers = checkBoxAcceptTransfers.Checked;
            MainSettings.saveSettings();
        }

        private void checkedListBoxAlerts_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            bool checkedy = false;
            if (e.NewValue == CheckState.Checked)
                checkedy = true;

            switch (e.Index)
            {
                case 0:
                    Settings.Default.PopupOnNewMessage = checkedy;
                    break;
                case 1:
                    Settings.Default.SoundOnNewMessage = checkedy;
                    break;
                case 2:
                    Settings.Default.SpeakMessageIsFrom = checkedy;
                    break;
                case 3:
                    Settings.Default.SpeakMessage = checkedy;
                    break;
                case 4:
                    Settings.Default.AlwaysSpeakMessages = checkedy;
                    break;
                case 5:
                    Settings.Default.PopupOnNewFileArrival = checkedy;
                    break;
                case 6:
                    Settings.Default.SoundOnNewFileArrival = checkedy;
                    break;
                case 7:
                    Settings.Default.SpeakOnNewFileArrival = checkedy;
                    break;
                case 8:
                    Settings.Default.SpeakNameWhenUserComesOnline = checkedy;
                    break;
                case 9:
                    Settings.Default.SoundWhenUserComesOnline = checkedy;
                    break;
                case 10:
                    Settings.Default.PopupWhenUserComesOnline = checkedy;
                    break;
            }

            saveSettings();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (networkChanges)
            {
                Settings.Default.ChatPort = numericUpDownChatPort.Value;
                Settings.Default.FilePort = numericUpDownFilePort.Value;
                Settings.Default.Timeout = numericUpDownTimeout.Value;
                Settings.Default.BroadcastPort = numericUpDownBroadcastPort.Value;
           

                IPAddress ip;
                if (IPAddress.TryParse(textBoxBroadcastAddress.Text.Trim(), out ip))
                    Settings.Default.BroadcastAddress = textBoxBroadcastAddress.Text.Trim();
                else
                {
                    MessageBox.Show("Broadcast address is incorrect", "Broadcast address is incorrect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

               // saveSettings();
            }

         
            Settings.Default.StartWithWindows = checkBox1.Checked;
            Registry.StartWithWindows(checkBox1.Checked);

            Settings.Default.StartMinimized = checkBox2.Checked;

            Settings.Default.AwayStatusOnScreenSaver = checkBox3.Checked;

            Settings.Default.ShowInTaskbar = !checkBox4.Checked;
                    

            saveSettings();

            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.SoundFileLocation = openFileDialog1.FileName;
                saveSettings();

                textBoxSoundLocation.Text = Settings.Default.SoundFileLocation;
            }
        }

        private void buttonDefaultFont_Click(object sender, EventArgs e)
        {
            fontDialog1.Font = Settings.Default.DefaultFont;
            if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.DefaultFont = fontDialog1.Font;
                buttonDefaultFont.Font = fontDialog1.Font;
                saveSettings();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = Settings.Default.DefaultFontColor;
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.DefaultFontColor = colorDialog1.Color;
                buttonForeColor.BackColor = colorDialog1.Color;
                saveSettings();
            }
        }

        private void buttonBackColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = Settings.Default.DefaultBackColor;
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.DefaultBackColor = colorDialog1.Color;
                buttonBackColor.BackColor = colorDialog1.Color;
                saveSettings();
            }
        }

        private void checkBoxEnableLogging_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableLogging = checkBoxEnableLogging.Checked;
            saveSettings();
        }

        private void radioButtonUseShare_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonUseShare.Checked)
            {

                Settings.Default.UseLocalTasksNotFtp = true;
                saveSettings();

                buttonSelectShare.Enabled = true;
                textBoxTaskFile.Enabled = true;

                textBoxFTPHost.Enabled = false;
                numericUpDownPort.Enabled = false;
                textBoxFTPUser.Enabled = false;
                textBoxFtpPassword.Enabled = false;
            }
        }

        private void radioButtonuseFtp_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonuseFtp.Checked)
            {
                Settings.Default.UseLocalTasksNotFtp = false;
                saveSettings();

                buttonSelectShare.Enabled = false;
                textBoxTaskFile.Enabled = false;

                textBoxFTPHost.Enabled = true;
                numericUpDownPort.Enabled = true;
                textBoxFTPUser.Enabled = true;
                textBoxFtpPassword.Enabled = true;

               
            }
        }

        private void buttonSelectTransfersFolder_Click(object sender, EventArgs e)
        {
            string path = Settings.Default.TransfersFolder;
            if (path == "")
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = docs + @"\NetworkChat\files\";
                

            }


            folderBrowserDialog1.SelectedPath = path;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxTransferFolder.Text = folderBrowserDialog1.SelectedPath;
                Settings.Default.TransfersFolder = folderBrowserDialog1.SelectedPath;
                saveSettings();
            }
        }

        private void textBoxEmail_TextChanged(object sender, EventArgs e)
        {
            //Settings.Default.Email = textBoxEmail.Text;
            //saveSettings();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            bool reg = Settings.Default.Registered;
            Settings.Default.Reset();
            Settings.Default.Registered = reg;
            Settings.Default.Save();
            Settings.Default.Reload();

            MessageBox.Show("Settings have been reset", "Settings have been reset", MessageBoxButtons.OK, MessageBoxIcon.Information);

            loadSettings();

            networkChanges = true;
        }


        
        private void numericUpDownChatPort_ValueChanged_1(object sender, EventArgs e)
        {
            
        }

        private void textBoxBroadcastAddress_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void numericUpDownChatPort_Enter(object sender, EventArgs e)
        {
             networkChanges = true;
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            //CheckBox cb = (CheckBox)sender;
            //bool checkedy = false;
            //if (cb.Checked)
            //    checkedy = true;

            //settingsObject settingsObject = new MainSettings.settingsObject();
            //settingsObject.Name = cb.Name;
            //settingsObject.Checked = checkedy;
            //ParameterizedThreadStart pst = new ParameterizedThreadStart(changeSettings);
            //Thread t = new Thread(pst);
            //t.Start(settingsObject);
           
        }

        class settingsObject
        {
            public string Name { get; set; }
            public bool Checked { get; set; }
        }

        void changeSettings(object o)
        {
            settingsObject settingsObject = (settingsObject)o;
            bool checkedy = settingsObject.Checked;
            switch (settingsObject.Name)
            {
                case "checkBox1":
                    Settings.Default.StartWithWindows = checkedy;
                    Registry.StartWithWindows(checkedy);
                    break;

                case "checkBox2":
                    Settings.Default.StartMinimized = checkedy;
                    break;

                case "checkBox3":
                    Settings.Default.AwayStatusOnScreenSaver = checkedy;
                    break;

                case "checkBox4":
                    Settings.Default.ShowInTaskbar = !checkedy;
                    break;

            }



            saveSettings();
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //Settings.Default.StartWithWindows = checkBox1.Checked;
            //Registry.StartWithWindows(checkBox1.Checked);

            //saveSettings();
        }

        private void textBoxFTPHost_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.FTPHost = textBoxFTPHost.Text.Trim();
            saveSettings();
        }

        private void textBoxFTPUser_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.FTPUser = textBoxFTPUser.Text.Trim();
            saveSettings();
        }

        private void textBoxFtpPassword_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.FTPPassword = textBoxFtpPassword.Text.Trim();
            saveSettings();
        }

        private void numericUpDownPort_ValueChanged(object sender, EventArgs e)
        {
            Settings.Default.FTPPort = (int)numericUpDownPort.Value;
            saveSettings();
        }

      
    }
}
