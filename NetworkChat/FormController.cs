using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NetworkChat.Properties;
using System.Resources;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Net;
using System.Media;

namespace NetworkChat
{
    public partial class Form1 
    {
        private List<Lan.NetworkUser> currentUsers = new List<Lan.NetworkUser>();
        private Lan.NetworkUser currentlySelectedUser;
        private UserMessages userMessages = new UserMessages();

        delegate void dUserPulse(Lan.NetworkUser NetworkUser);
        delegate void dInvokeMessage(Lan.NetworkUser Computer, string Message);
        delegate void dsendMessage(Lan.NetworkUser User, string Message);
        delegate void dMessageSent();
        delegate void dFileSent(int FilesCount);

        private List<Lan.NetworkUser> usersWithMessages = new List<Lan.NetworkUser>();

        Chat chat = new Chat();

        

        private void userSwitchOff()
        {
            timerBroadcast.Enabled = false;
            Lan.ShuttingDown = true;
            Lan.ShutDown();
            Chat.StopListener();

        }

        private void userSwitchOn()
        {
            

            ThreadStart ts = new ThreadStart(ListenForBroadcast);
            Thread t = new Thread(ts);
            t.IsBackground = true;
            t.Start();

            ts = new ThreadStart(startListener);
            t = new Thread(ts);
            t.Start();

            //start listen for file
            ts = new ThreadStart(startListenerForFiles);
            t = new Thread(ts);
            t.Start();

            refreshTasks();

           
            timerBroadcast.Enabled = true;

        }


        void startUp()
        {

            //Settings.Default.Reset();

            this.ShowInTaskbar = Settings.Default.ShowInTaskbar;

            if (Settings.Default.StartMinimized)
                this.WindowState = FormWindowState.Minimized;

            updateRichMessageBoxes();
            loadMessages("Broadcast");

            this.Size = Settings.Default.FormSize;

            if (Settings.Default.FormLocation.X > -1 & Settings.Default.FormLocation.Y > -1)
                this.Location = Settings.Default.FormLocation;
            else
            {
                //center
                int HalfHeight = this.Height / 2;
                int HalfWidth = this.Width / 2;
                Point p = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - HalfWidth, Screen.PrimaryScreen.Bounds.Height / 2 - HalfHeight);
                this.Location = p;
            }

            userSwitchOn();
            LoadStatusCombo();

            loadStatuses();

            notifyIcon1.BalloonTipClicked += new EventHandler(notifyIcon1_BalloonTipClicked);

            Lan.NetworkUserPulse += new Lan.dNetworkUser(Lan_NetworkUserPulse);
            Chat.NewMessage += new Chat.d_NewMessage(Chat_NewMessage);
            chat.UserOffline += new Chat.d_UserOffline(chat_UserOffline);
            Chat.NewFile += new Chat.d_NewFile(Chat_NewFile);
            chat.MessageSent += new Chat.d_MessageSent(Chat_MessageSent);
            Chat.FileSent += new Chat.d_FileSent(Chat_FileSent);

            MainSettings.MessagesDeleted += new MainSettings.dDeletingMessages(MainSettings_MessagesDeleted);

            UserGroups.LoadUserGroups();
            refreshGroups();


            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

           // Application.Idle += new EventHandler(Application_Idle);
            

            startBroadcast((Lan.Presence)Enum.Parse(typeof(Lan.Presence), Settings.Default.CurrentPresence));

            //user set
            // ThreadStart ts = new ThreadStart(startMulticast);
            // Thread t = new Thread(ts);
            // t.Start();

            // Thread.Sleep(1000);

            // MultiCastClient.JoinMulticastGroup(Lan.GetLocalComputer().IPAddress, MultiCastServer.getMulticastIPs()[0]);

            //// MultiCastClient.BroadCastPresence(NetworkChat.Lan.Presence.Busy);

            // MultiCastClient.BroadCastPresence(NetworkChat.Lan.Presence.Away);

        }

       

        void MainSettings_MessagesDeleted()
        {
            richTextBoxReplies.Text = "";
        }

        void Chat_FileSent(int FilesCount)
        {
            Invoke(new dFileSent(fileSent), new object[] { FilesCount });
            
        }

        void fileSent(int FilesCount)
        {
            toolStripProgressBar1.Maximum = FilesCount;
            if (toolStripProgressBar1.Value == FilesCount)
            {
                toolStripProgressBar1.Value = 1;
                
            }
            else
                toolStripProgressBar1.Value++;

           
        }

        private void ListenForBroadcast()
        {
            Lan.ShuttingDown = false;
            Lan.ListenForBroadcast(IPAddress.Parse(Settings.Default.BroadcastAddress), (int)Settings.Default.BroadcastPort);
        }

        private void startListener()
        {
            Lan.ShuttingDown = false;
            IPAddress ip = Lan.GetLocalComputer().IPAddress;
            Chat.Listen(ip);

        }

        private void startListenerForFiles()
        {
            IPAddress ip = Lan.GetLocalComputer().IPAddress;
            Chat.ListenForFile(ip);
        }


        private void checkUsersOnline()
        {
            foreach (NetworkChat.Lan.NetworkUser nu in currentUsers)
            {
                if (!Lan.IsUserThere(nu))
                {
                    listViewUsers.Items[nu.UserUniqueId].Remove();
                }

                refreshGroups();

            }
        }

        private void startBroadcast(Lan.Presence Presence)
        {
            ParameterizedThreadStart pst = new ParameterizedThreadStart(BroadcastPresence);
            Thread t = new Thread(pst);
            t.Start(Presence);
        }

        private void BroadcastPresence(object o)
        {
            Lan.Presence Presence = (Lan.Presence)o;
            Lan.BroadcastPresence(Presence);

        }

        private void startMulticast()
        {
            // Start a multicast group.
            MultiCastServer.StartMulticast(Lan.GetLocalComputer().IPAddress, MultiCastServer.getMulticastIPs()[0]);
            // Receive broadcast messages.
            MultiCastServer.ReceiveBroadcastMessages();
        }


        private void LoadStatusCombo()
        {
            foreach (TasksLocalShare.TaskStatus t in Enum.GetValues(typeof(TasksLocalShare.TaskStatus)))
            {
                comboBoxStatus.Items.Add(t);
            }

            comboBoxStatus.SelectedIndex = 0;
        }


        void Lan_NetworkUserPulse(Lan.NetworkUser NetworkUser)
        {
            try
            {
                if (!this.IsDisposed)
                    Invoke(new dUserPulse(userPulse), NetworkUser);
            }
            catch
            {

            }
        }

        void Chat_MessageSent()
        {
            Invoke(new dMessageSent(messageSent));

        }

        void messageSent()
        {
          
            //buttonSend.Enabled = true;
        }

        void Chat_NewFile(string FromID, string From, string FileName)
        {
          
                if (Settings.Default.PopupOnNewFileArrival)
                {
                    notifyIcon1.ShowBalloonTip(5000, "New File From " + From, "You have a file called " + FileName + " from " + From, ToolTipIcon.Info);
                    notifyIcon1.Tag = new string[] { FromID, FileName };

                }

                if (Settings.Default.SoundOnNewFileArrival)
                {
                    SoundPlayer sp ;
                    if (Settings.Default.SoundFileLocation != "")
                    {
                        if (File.Exists(Settings.Default.SoundFileLocation))
                        {
                            sp = new SoundPlayer(Settings.Default.SoundFileLocation);
                            sp.Play();
                        }
                        else
                        {
                            SystemSounds.Beep.Play();

                        }
                    }
                    else
                    {
                        SystemSounds.Beep.Play();
                        
                    }
                        
                }

                if (Settings.Default.SpeakOnNewFileArrival)
                    Speak.SpeakPhrase("You have a file called " + FileName + " from " + From);
            
        }


        private void whereToSendMessage()
        {
            if (richMessage.Text.Trim() == "")
            {
                MessageBox.Show("Please write a message", "Please write a message", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            //first check if its a group or user.
            object o = tabPage1.Tag;


            if (o.GetType() == typeof(Lan.NetworkUser))
            {
                //if user is offline then dont send anyway
                //if (currentlySelectedUser.Presence == Lan.Presence.Offline)
                //{
                //    MessageBox.Show(currentlySelectedUser.UserName + " is offline. Please try later", currentlySelectedUser.UserName + " is offline", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //    return;
                //}

                startSendMessage(currentlySelectedUser);

                //TODO ONly save if user says so
                
                userMessages.SaveUserMessageText(((Lan.NetworkUser)o).UserUniqueId, richTextBoxReplies.Rtf);
            }
            else
            {

                startSendGroup();
                //TODO ONly save if user says so
                
                userMessages.SaveUserMessageText(o.ToString(), richTextBoxReplies.Rtf);
            }

            //clear here,,,
            toolStripComboBoxAttachments.Items.Clear();
            toolStripComboBoxAttachments.Text = "";
            richMessage.Rtf = "";
            richMessage.Select(0, 0);

        }

        private void adjustAfterMessageSent()
        {
          
        }

        private void startSendMessage(Lan.NetworkUser nu)
        {

            if (nu == null)
                return;

            MessageObject me = new MessageObject();
            me.User = nu;
            me.User.TimeStamp = DateTime.Now;
            me.Message = richMessage.Rtf; //richMessage.Rtf;

            //display issues
            //richMessage.Rtf = "";
            //richMessage.Select(0, 0);
            richTextBoxReplies.Select(richTextBoxReplies.Text.Length, 0);
            richTextBoxReplies.SelectedRtf = @"{\rtf1\ansi\deff0{\colortbl;\red0\green0\blue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red255\green255\blue0;\red255\green255\blue255;\red0\green0\blue128;\red0\green128\blue128;\red0\green128\blue0;\red128\green0\blue128;\red128\green0\blue0;\red128\green128\blue0;\red128\green128\blue128;\red192\green192\blue192;}" + @"\cf11[" + DateTime.Now.ToShortTimeString() + "] " + Environment.UserName + @": }";
            richTextBoxReplies.Select(richTextBoxReplies.Text.Length, 0);
            richTextBoxReplies.SelectedRtf = me.Message.Trim();
            richTextBoxReplies.ScrollToCaret();

          

            ParameterizedThreadStart pst = new ParameterizedThreadStart(sendMessage);
            Thread t = new Thread(pst);
            t.Start(me);

        }

        private void startSendGroup()
        {
            string groupName = tabPage1.Tag.ToString();

            if (groupName == "Broadcast")
            {
                //send to all online
                //currentUsers
                foreach (Lan.NetworkUser nuw in currentUsers)
                {
                    if (nuw.Presence != Lan.Presence.Offline)
                        startSendMessage(nuw);
                }
            }
            else
            {
                //send to all in group...
                List<GroupUser> users = UserGroups.GetGroupMembers(groupName);
                foreach (GroupUser gu in users)
                {
                    Lan.NetworkUser nu = currentUsers.Find(s => s.UserUniqueId == gu.UserUniqueId);

                    if (nu != null)
                        if (nu.Presence != Lan.Presence.Offline)
                            startSendMessage(nu);
                }

            }

           
        }

        private void addFilesToRTF()
        {
            if (toolStripComboBoxAttachments.Items.Count > 0)
            {
                //add file attaches to the rtf...
                for (int i = 0; i < toolStripComboBoxAttachments.Items.Count; i++)
                {
                    FileObject fo = (FileObject)toolStripComboBoxAttachments.Items[i];
                    richMessage.Select(richMessage.TextLength, 0);
                    richMessage.SelectedRtf = @"{\rtf1\ansi\deff0{\colortbl;\red0\green0\blue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red255\green255\blue0;\red255\green255\blue255;\red0\green0\blue128;\red0\green128\blue128;\red0\green128\blue0;\red128\green0\blue128;\red128\green0\blue0;\red128\green128\blue0;\red128\green128\blue128;\red192\green192\blue192;}" + @"\line\cf11[" + DateTime.Now.ToShortTimeString() + @"] \cf11" + Environment.UserName + @":\cf9 Attached File: " + fo.FileName + " }";
                    richMessage.ScrollToCaret();
                }
            }
        }

        private void sendMessage(object o)
        {
            MessageObject me = (MessageObject)o;

            //get attachments
            string[] files = new string[toolStripComboBoxAttachments.Items.Count];

            if (files.Length > 0)
            {
                for (int i = 0; i < toolStripComboBoxAttachments.Items.Count; i++)
                {
                    FileObject fo = (FileObject)toolStripComboBoxAttachments.Items[i];
                    files[i] = fo.FilePath;
                }

                
            }


            chat.SendMessage(me.User, me.Message, files);
        }

        void Chat_NewMessage(Lan.NetworkUser Computer, string Message)
        {
            if (!this.IsDisposed)
                Invoke(new dInvokeMessage(invokeMessage), new object[] { Computer, Message });
        }

        private void invokeMessage(Lan.NetworkUser User, string Message)
        {


            //ONLY do this if the user is present
            object o = tabPage1.Tag;
            bool showOnScreen = false;
            if (o.GetType() == typeof(NetworkChat.Lan.NetworkUser))
            {
                NetworkChat.Lan.NetworkUser nu = (NetworkChat.Lan.NetworkUser)o;
                if (nu.UserUniqueId.ToLower() == User.UserUniqueId)
                    showOnScreen = true;
            }
            else //group
            {
            }


            if (showOnScreen)
            {
                richTextBoxReplies.Select(richTextBoxReplies.TextLength, 0);
                //use custom colors for users...
                richTextBoxReplies.SelectedRtf = @"{\rtf1\ansi\deff0{\colortbl;\red0\green0\blue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red255\green255\blue0;\red255\green255\blue255;\red0\green0\blue128;\red0\green128\blue128;\red0\green128\blue0;\red128\green0\blue128;\red128\green0\blue0;\red128\green128\blue0;\red128\green128\blue128;\red192\green192\blue192;}" + @"\cf2[" + User.TimeStamp.ToShortTimeString() + "] " + User.UserName + @": }";
                richTextBoxReplies.Select(richTextBoxReplies.TextLength, 0);

                richTextBoxReplies.SelectedRtf = Message;
                richTextBoxReplies.ScrollToCaret(); //.Select(richTextBox1.TextLength, 0);

                if (Settings.Default.AlwaysSpeakMessages)
                {
                    RichTextBox ritb = new RichTextBox();
                    ritb.Rtf = Message;
                    Speak.SpeakPhrase(User.UserName + " says: " + ritb.Text);
                }

            }
            else if(!showOnScreen | !this.TopMost | this.WindowState == FormWindowState.Minimized)//not shown on screen just flash....
            {
                //add user to usersWithMessages;
                notifyIcon1.Tag = User;

                if (Settings.Default.PopupOnNewMessage)
                    notifyIcon1.ShowBalloonTip(5000, "You have a new message", "You have a new message from " + User.UserName, ToolTipIcon.Info);

                if (Settings.Default.SoundOnNewMessage)
                {
                    if (Settings.Default.SoundFileLocation != "")
                    {
                        if (File.Exists(Settings.Default.SoundFileLocation))
                        {
                            SoundPlayer sp = new SoundPlayer(Settings.Default.SoundFileLocation);
                            sp.Play();
                        }
                        else
                        {
                            SystemSounds.Beep.Play();

                        }
                    }
                    else
                    {
                        SystemSounds.Beep.Play();

                    }
                }

                if (Settings.Default.SpeakMessage | Settings.Default.AlwaysSpeakMessages)
                {
                    RichTextBox ritb = new RichTextBox();
                    ritb.Rtf = Message;
                    Speak.SpeakPhrase("You have a message from " + User.UserName + ", which says: " + ritb.Text);
                }
                else if (Settings.Default.SpeakMessageIsFrom)
                    Speak.SpeakPhrase("You have a new message from " + User.UserName);

              


                usersWithMessages.Add(User);


            }

            //get old string
            //save as user who sent

           
                RichTextBox rtb = new RichTextBox();
                string old = userMessages.LoadUserMessageText(User.UserUniqueId);
                rtb.Rtf = old;
                rtb.Select(rtb.TextLength, 0);

                rtb.SelectedRtf = @"{\rtf1\ansi\deff0{\colortbl;\red0\green0\blue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red255\green255\blue0;\red255\green255\blue255;\red0\green0\blue128;\red0\green128\blue128;\red0\green128\blue0;\red128\green0\blue128;\red128\green0\blue0;\red128\green128\blue0;\red128\green128\blue128;\red192\green192\blue192;}" + @"\cf2[" + User.TimeStamp.ToShortTimeString() + "] " + User.UserName + @": " + Message + " }";
                richTextBoxReplies.ScrollToCaret();


                userMessages.SaveUserMessageText(User.UserUniqueId, rtb.Rtf);
            
        }
        

        private void refreshAfterUserSelect(Lan.NetworkUser nu)
        {
            int index = listViewUsers.Items.IndexOfKey(nu.UserUniqueId);
            currentlySelectedUser = nu;
            //go through listviewuser and if the same
            if (index > -1)
            {
                listViewUsers.Items[nu.UserUniqueId].Selected = true;
                listViewUsers.Items[nu.UserUniqueId].BackColor = Color.LightPink;
                listViewUsers.Items[nu.UserUniqueId].ForeColor = Color.Purple;

                tabPage1.Tag = nu;
                tabPage1.Text = nu.UserName + " Messages";
                buttonSend.Text = "Send To " + nu.UserName;

                tabControl1.SelectedIndex = 0;

                string text = userMessages.LoadUserMessageText(nu.UserUniqueId);
                richTextBoxReplies.Rtf = text;

                richTextBoxReplies.Select(richTextBoxReplies.TextLength, 0);

                richTextBoxReplies.ScrollToCaret();

                toolStripButtonBold.Checked = Settings.Default.DefaultFont.Bold;
                toolStripButtonItalic.Checked = Settings.Default.DefaultFont.Italic;
                toolStripButtonUnderline.Checked = Settings.Default.DefaultFont.Underline;
                toolStripButtonStrikeOut.Checked = Settings.Default.DefaultFont.Strikeout;

                richMessage.Font = Settings.Default.DefaultFont;
                richMessage.SelectionFont = Settings.Default.DefaultFont;

                foreach (ListViewItem lvi in listViewUsers.Items)
                {
                    lvi.BackColor = Color.White;
                    lvi.ForeColor = Color.Black;
                }

                if (listViewUsers.SelectedItems.Count > 0)
                {
                    listViewUsers.SelectedItems[0].BackColor = Color.LightPink;
                    listViewUsers.SelectedItems[0].ForeColor = Color.Purple;
                }

                foreach (TreeNode tn in treeViewGroups.Nodes)
                {
                    tn.BackColor = Color.White;
                    tn.ForeColor = Color.Black;
                }

                Lan.NetworkUser nit = usersWithMessages.Find(s => s.UserUniqueId == nu.UserUniqueId.ToLower());
                usersWithMessages.Remove(nit);

                richMessage.Select();

                this.WindowState = FormWindowState.Normal;
                this.BringToFront();

            }
        }

        private void loadStatuses()
        {
            string[] names = Enum.GetNames(typeof(NetworkChat.Lan.Presence));
            ResourceManager rm = Resources.ResourceManager;
            Bitmap img;//

            foreach (string name in names)
            {
                img = (Bitmap)rm.GetObject(name.ToLower());
                ToolStripItem tsi = toolStripDropDownButtonStatus.DropDownItems.Add(name, img);
                tsi.Tag = name;
                tsi.Click += new EventHandler(tsi_Click);


                tsi = statusToolStripMenuItemStatus.DropDownItems.Add(name, img);
                tsi.Tag = name;
                tsi.Click += new EventHandler(tsi_Click);
                statusToolStripMenuItemStatus.DropDownItems.Add(tsi);

            }

            //set default
            img = (Bitmap)rm.GetObject(Settings.Default.CurrentPresence.ToLower());
            toolStripDropDownButtonStatus.Image = img;
            statusToolStripMenuItemStatus.Image = img;

        }


        private void userPulse(Lan.NetworkUser NetworkUser)
        {

            bool found = false;
            string pres = Enum.GetName(typeof(Lan.Presence), NetworkUser.Presence).ToLower();

            for (int i = 0; i < listViewUsers.Items.Count; i++)
            {
                //store in global
                Lan.NetworkUser nu = (Lan.NetworkUser)listViewUsers.Items[i].Tag;

                if (nu.UserUniqueId == NetworkUser.UserUniqueId)// & (nu.ComputerName == NetworkUser.ComputerName))
                {
                    //monitor status changes here
                    //***************************
                    currentUsers.RemoveAll(s => s.UserUniqueId == nu.UserUniqueId);
                    currentUsers.Add(NetworkUser);

                    IEnumerator tss = sendMessageToolStripMenuItemSendMessage.DropDownItems.GetEnumerator();

                    while (tss.MoveNext())
                    {
                        //if(tss.Current.
                        ToolStripItem t = (ToolStripItem)tss.Current;

                        Lan.NetworkUser nut = (Lan.NetworkUser)t.Tag;
                        if (nut.UserUniqueId == NetworkUser.UserUniqueId)
                        {

                            ResourceManager rm = Resources.ResourceManager;
                            Image img = (Image)rm.GetObject(pres);
                            t.Tag = NetworkUser;
                            t.Image = img;
                        }

                    }


                    found = true;
                }


            }

            if (!found)
            {


                ListViewItem lvi = listViewUsers.Items.Add(NetworkUser.UserUniqueId, "", pres);
                lvi.SubItems.Add(NetworkUser.UserName);
                //lvi.SubItems.Add(pres);
                lvi.Tag = NetworkUser;

                ToolStripItem tsi = sendMessageToolStripMenuItemSendMessage.DropDownItems.Add(NetworkUser.UserName);
                tsi.Click += new EventHandler(tsiMenuSendMessage_Click);
                ResourceManager rm = Resources.ResourceManager;
                Image img = (Image)rm.GetObject(pres);
                tsi.Tag = NetworkUser;
                tsi.Image = img;

                currentUsers.Add(NetworkUser);

                if (Settings.Default.SoundWhenUserComesOnline)
                {
                    if (Settings.Default.SoundFileLocation != "")
                    {
                        if (File.Exists(Settings.Default.SoundFileLocation))
                        {
                            SoundPlayer sp = new SoundPlayer(Settings.Default.SoundFileLocation);
                            sp.Play();
                            Thread.Sleep(50);
                        }
                        else
                        {
                            SystemSounds.Beep.Play();

                        }
                    }
                    else
                    {
                        SystemSounds.Beep.Play();

                    }
                }

                if (Settings.Default.PopupWhenUserComesOnline)
                {
                    notifyIcon1.Tag = null;
                    if (NetworkUser.Presence == Lan.Presence.Online)
                        notifyIcon1.ShowBalloonTip(5000, NetworkUser.UserName + " just came online", NetworkUser.UserName + " is now online on PC: " + NetworkUser.ComputerName, ToolTipIcon.Info);

                }

                if (Settings.Default.SpeakNameWhenUserComesOnline)
                {
                    Speak.SpeakPhrase(NetworkUser.UserName + " is now online");
                }

               
            }

            refreshUsers();
            refreshGroups();
        }


        private void refreshUsers()
        {
            int selIndex = 0;
            if (listViewUsers.SelectedIndices.Count > 0)
                selIndex = listViewUsers.SelectedItems[0].Index;

            NetworkChat.Lan.NetworkUser[] nus = new Lan.NetworkUser[currentUsers.Count];
            currentUsers.CopyTo(nus);
            foreach (NetworkChat.Lan.NetworkUser nu in nus)
            {

                //    listViewUsers.Items[nu.UserUniqueId].SubItems[2].Text = nu.Presence.ToString();
                listViewUsers.Items[nu.UserUniqueId].ImageKey = nu.Presence.ToString().ToLower();


            }


        }

        delegate void dRefreshTasks(List<TasksLocalShare.Task> Tasks, int selected = 0);
        private void doRefreshTasks(List<TasksLocalShare.Task> Tasks, int selected = 0)
        {
            dataGridView1.Rows.Clear();

            foreach (TasksLocalShare.Task task in Tasks)
            {

                int i = dataGridView1.Rows.Add(new object[] { task.TaskTitle, task.LastEditDate.ToString(), task.LastEditBy });
                dataGridView1.Rows[i].Tag = task;

            }

            SetGridSelect(selected);
        }
        private void refreshTasks(int selected = 0)
        {
            

            if (Settings.Default.UseLocalTasksNotFtp)
            {
                List<TasksLocalShare.Task> tasks = TasksLocalShare.LoadTaskList();
                if (tasks != null)
                {
                    //listView1.Items.Clear();
                    if (InvokeRequired)
                        Invoke(new dRefreshTasks(doRefreshTasks), new object[] { tasks, selected });
                    else
                        doRefreshTasks(tasks, selected);
                    
                }
                else
                {
                    //MainSettings ms = new MainSettings();
                    //ms.ShowDialog();
                    //refreshTasks();
                }
            }
            else if (Settings.Default.UseLocalTasksNotFtp == false)
            {
                //if (Settings.Default.FTPHost != "")
                //    FtpTasks.CreateTaskFile();
            }
        }


        private void refreshGroups()
        {

            Dictionary<string, UserGroup> grps = UserGroups.ViewGroups();

            foreach (string kk in grps.Keys)
            {
                UserGroup ug = grps[kk];
                TreeNode tn;

                if (!treeViewGroups.Nodes.ContainsKey(kk))
                {
                    tn = treeViewGroups.Nodes.Add(ug.GroupName, ug.GroupName);
                    tn.ImageKey = "group";
                    tn.SelectedImageKey = "group";
                    tn.Tag = "GROUP";
                }
                else
                    tn = treeViewGroups.Nodes[kk];

                bool isExpended = tn.IsExpanded;

                foreach (GroupUser gu in ug.GroupMembers)
                {
                    Lan.NetworkUser nu = currentUsers.Find(s => s.UserUniqueId == gu.UserUniqueId);

                    //oly shows online users...???
                    if (nu != null)
                    {
                        //check if in tree
                        if (!tn.Nodes.ContainsKey(nu.UserUniqueId))
                        {
                            //check if user online...
                            TreeNode unode = tn.Nodes.Add(nu.UserUniqueId, gu.UserName);
                            unode.ImageKey = "person";
                            unode.SelectedImageKey = "person";
                            unode.Tag = gu;

                        }
                    }
                    else
                    {
                        if (tn.Nodes.ContainsKey(gu.UserUniqueId))
                        {
                            //Also remove from user list..
                            tn.Nodes[gu.UserUniqueId].Remove();
                        }

                    }

                    if (isExpended)
                        tn.Expand();

                }
            }
        }

        private void showSettings()
        {
            MainSettings ms = new MainSettings();
            ms.ShowDialog();

            ThreadStart ts = new ThreadStart(userSwitchIT);
            Thread t = new Thread(ts);
            t.Start();
          

            updateRichMessageBoxes();
            refreshTasks();

            
        }

        void userSwitchIT()
        {
            userSwitchOff();
            userSwitchOn();
        }

        private void updateRichMessageBoxes()
        {
            richMessage.SelectionFont = Settings.Default.DefaultFont;
            richMessage.SelectionColor = Settings.Default.DefaultFontColor;

            richMessage.Font = Settings.Default.DefaultFont;
            richMessage.ForeColor = Settings.Default.DefaultFontColor;

            richMessage.BackColor = Settings.Default.DefaultBackColor;
        }


        private void SetGridSelect(int selected = 0)
        {
            if (selected > 0)
                dataGridView1.Rows[selected].Selected = true;
            else if (dataGridView1.SelectedRows == null & dataGridView1.Rows.Count > 0)
                dataGridView1.Rows[0].Selected = true;

            if (dataGridView1.SelectedRows.Count > 0)
            {
                //bring task direct from file here
                TasksLocalShare.Task task = (TasksLocalShare.Task)dataGridView1.SelectedRows[0].Tag;
                textBoxContent.Text = task.TaskDetails;
                textBoxContent.Tag = task;
                labelEditDate.Text = task.LastEditDate.ToString();
                comboBoxStatus.SelectedItem = (object)task.TaskStatus;


            }
        }

        private void selectFont()
        {
            fontDialog1.Font = richMessage.SelectionFont;
            if (DialogResult.OK == fontDialog1.ShowDialog())
            {
                richMessage.SelectionFont = fontDialog1.Font;
                toolStripButtonBold.Checked = richMessage.SelectionFont.Bold;
                toolStripButtonItalic.Checked = richMessage.SelectionFont.Italic ;
                toolStripButtonUnderline.Checked = richMessage.SelectionFont.Underline;
                toolStripButtonStrikeOut.Checked = richMessage.SelectionFont.Strikeout;

                
            }
        }


        public class FileObject
        {
            public string FileName { get; private set; }
            public string FilePath { get; private set; }
            public FileObject(string path)
            {
                FileInfo fi = new FileInfo(path);
                this.FileName = fi.Name;
                this.FilePath = path;
            }

            public override string ToString()
            {
                return FileName;
            }
        }

        class MessageObject
        {
            public Lan.NetworkUser User { get; set; }
            public string Message { get; set; }
        }

        private Font ToggleFontStyle(Font OriginalFont, FontStyle Style)
        {
            bool turnOn = true;
            switch (Style)
            {
                case FontStyle.Bold:
                    if (OriginalFont.Bold)
                        turnOn = false;
                    break;
                case FontStyle.Italic:
                    if (OriginalFont.Italic)
                        turnOn = false;
                    break;
                case FontStyle.Underline:
                    if (OriginalFont.Underline)
                        turnOn = false;
                    break;
                case FontStyle.Strikeout:
                    if (OriginalFont.Strikeout)
                        turnOn = false;
                    break;
            }

            Font f;
            if (turnOn)
                f = new Font(OriginalFont, OriginalFont.Style | Style);
            else
                f = new Font(OriginalFont, OriginalFont.Style ^ Style);
            return f;
        }

        //private void refreshTreeViewGroups(int HighlightNode = 0)
        //{
        //    //if (treeViewGroups.SelectedNode != null)
        //    //{
        //    foreach (TreeNode tn in treeViewGroups.Nodes)
        //    {
        //        //if (tn != treeViewGroups.SelectedNode)
        //        //{
        //        tn.BackColor = Color.White;
        //        tn.ForeColor = Color.Black;
        //        // }
        //        //else
        //        //{
        //        //    treeViewGroups.SelectedNode.BackColor = Color.LightPink;
        //        //    treeViewGroups.SelectedNode.ForeColor = Color.Purple;
        //        //}
        //    }

        //    treeViewGroups.Nodes[HighlightNode].BackColor = Color.LightPink;
        //    treeViewGroups.Nodes[HighlightNode].ForeColor = Color.Purple;

        //    // }


        //}
    }
}
