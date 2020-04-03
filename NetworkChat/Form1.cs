using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using NetworkChat.Properties;
using System.Resources;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace NetworkChat
{
    public partial class Form1 : Form
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xf020;
        private const int SC_SCREENSAVE = 0xf140;

        protected override void WndProc(ref Message m)
        {
           // Debug.WriteLine("Message: " + m.Msg.ToString());
            if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam.ToInt32() == SC_MINIMIZE)
                {
                    m.Result = IntPtr.Zero;
                    if (Settings.Default.ShowInTaskbar)
                        this.ShowInTaskbar = true;
                    else
                    {
                        Visible = false;
                        return;
                    }
                }

            }

           
            base.WndProc(ref m);

        }



        public Form1()
        {
            //Settings.Default.Reset();

            //Settings.Default.Save();

            //Settings.Default.Reload();

            InitializeComponent();

            startUp();

            DoRegistered();
        }


        

        void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Resume:
                    userSwitchOn();
                    DoRegistered();
                    break;

                case Microsoft.Win32.PowerModes.Suspend:
                    userSwitchOff();
                    break;

            }
        }

        void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case Microsoft.Win32.SessionSwitchReason.ConsoleDisconnect:
                    userSwitchOff();
                    break;

                case Microsoft.Win32.SessionSwitchReason.ConsoleConnect:
                    userSwitchOn();
                    DoRegistered();
                    break;

            }

            
        }

       
        void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            //could be a file string[] or a Lan.NetworkUser.
            object obj = notifyIcon1.Tag;
            if (obj != null)
            {
                Type type = obj.GetType();
                if (type == typeof(string[]))
                {
                    string[] args = (string[])notifyIcon1.Tag;
                    string from = args[0];
                    string fname = args[1];
                    string path = Settings.Default.TransfersFolder;
                    if (path == "")
                    {
                        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        path = docs + @"\NetworkChat\files";
                    }

                    path = path + @"\" + from + @"\" + fname;

                    Process.Start("explorer.exe", @"/select, " + path);
                }
                else if (type == typeof(Lan.NetworkUser))
                {
                    Lan.NetworkUser nu = (Lan.NetworkUser)notifyIcon1.Tag;
                    refreshAfterUserSelect(nu);

                }
            }
        }

       

        void tsi_Click(object sender, EventArgs e)
        {
            ToolStripItem tsi = (ToolStripItem)sender;
            Settings.Default.CurrentPresence = tsi.Tag.ToString();
            
            MainSettings.saveSettings();

            ResourceManager rm = Resources.ResourceManager;
            Bitmap img;//
            img = (Bitmap)rm.GetObject(Settings.Default.CurrentPresence.ToLower());
            toolStripDropDownButtonStatus.Image = img;

            statusToolStripMenuItemStatus.Image = img;
            
            startBroadcast((Lan.Presence)Enum.Parse(typeof(Lan.Presence),Settings.Default.CurrentPresence));
        }

        void chat_UserOffline(Lan.NetworkUser User)
        {
            //Lan.NetworkUser nu = currentUsers.Single(s => s.UserUniqueId == User.UserUniqueId);
            //if (nu != null)
            //    currentUsers.Remove(nu);
        }


       

        void tsiMenuSendMessage_Click(object sender, EventArgs args)
        {
            ToolStripItem tsi = (ToolStripItem)sender;
            Lan.NetworkUser nu = (Lan.NetworkUser)tsi.Tag;
            refreshAfterUserSelect(nu);


            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            addFilesToRTF();
            whereToSendMessage();

         
         
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoRegistered();

          //  startBroadcast(Lan.Presence.UnAvailable);
           // Thread.Sleep(1000);
            Lan.BroadcastPresence(Lan.Presence.Offline);
          //shut down after broadcast 
            Thread.Sleep(500);

            userSwitchOff();

            if (!Settings.Default.EnableLogging)
            {
                UserMessages.DeleteAllMessages();
            }
            

            //575, 468
            Settings.Default.FormSize = this.Size;
            Settings.Default.FormLocation = this.Location;
            Settings.Default.Save();
            Settings.Default.Reload();


        ////    MultiCastServer.CancelMultiCast();
        //    Chat.StopListener();
        //    // Chat.ShuttingDown = true;
        //    Lan.ShuttingDown = true;
        }

        private void timerBroadcast_Tick(object sender, EventArgs e)
        {
            //check for DoRegistered() every 2 hours

            if ((ScreenSaver.IsScreenSaverOn() & Settings.Default.AwayStatusOnScreenSaver))
                {
                    startBroadcast(Lan.Presence.Away);
                }
                else
                {
                    //change to user Presence chosen
                    startBroadcast((NetworkChat.Lan.Presence)Enum.Parse(typeof(NetworkChat.Lan.Presence), Settings.Default.CurrentPresence));
                }
            

        }


        //DateTime idleStart;
        //bool WeAreIdling;
        ////how long have we been idle
        //bool WeAreIdle()
        //{
        //    if(idleStart == null)
        //        idleStart = DateTime.Now;

        //    if (idleStart.AddSeconds(30) < DateTime.Now)
        //    {
        //        startBroadcast(Lan.Presence.Away);
        //        return true;
        //    }
            
        //    return false;
        //}

       
        //void Application_Idle(object sender, EventArgs e)
        //{
        //    WeAreIdling = true;
            
        //}


        private void richMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                //buttonSend.Enabled = false;
                addFilesToRTF();
                whereToSendMessage();
                e.Handled = true;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == colorDialog1.ShowDialog())
            {
                richMessage.SelectionColor = colorDialog1.Color;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            richMessage.SelectionFont = ToggleFontStyle(richMessage.SelectionFont, FontStyle.Bold);
            toolStripButtonBold.Checked = richMessage.SelectionFont.Bold;
        }

      
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            richMessage.SelectionFont = ToggleFontStyle(richMessage.SelectionFont, FontStyle.Italic);
            toolStripButtonItalic.Checked = richMessage.SelectionFont.Italic;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            selectFont();
        }
      
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            TaskForm tf = new TaskForm();
            tf.ShowDialog();

            refreshTasks();
        }

  

        private void button3_Click(object sender, EventArgs e)
        {
            //first check the file has not changed by someone else in the meantime
            
            TasksLocalShare.Task task = (TasksLocalShare.Task)textBoxContent.Tag;

            bool letsSave = true;
            Nullable<bool> hasTaskBeenChanged = TasksChecker.LocalTaskChanged(task);
            bool hasValue = hasTaskBeenChanged.HasValue;

            if (hasValue)
            {
                
                if (hasTaskBeenChanged.Value)
                    if (MessageBox.Show("This task has been changed since loading. Do you want to save anyway?", "This task has been changed since loading", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                        letsSave = false;
            }
            else
            {
                //has been delted - offer to save as new
                letsSave = false;
                MessageBox.Show("This task has been deleted while you where editing it", "This task has been deleted while you where editing it", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            if (letsSave)
            {
                task.TaskStatus = (TasksLocalShare.TaskStatus)comboBoxStatus.SelectedItem;
                task.TaskDetails = textBoxContent.Text;
                task.LastEditDate = DateTime.Now;
                task.LastEditBy = Environment.UserName;
                TasksLocalShare.UpdateTask(task);
                
            }

            refreshTasks(dataGridView1.SelectedRows[0].Index);

            
        }

      

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
                refreshTasks();
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            //delete item
            if(dataGridView1.SelectedRows.Count > 0)
            {
                TasksLocalShare.Task task = (TasksLocalShare.Task)dataGridView1.SelectedRows[0].Tag;
                
                int index = dataGridView1.SelectedRows[0].Index-1;

                dataGridView1.Rows.Remove(dataGridView1.SelectedRows[0]);
                //dataGridView1.Rows[0].Selected = true;
                //now delete from xml and refresh
                TasksLocalShare.DeleteTask(task.TaskId);

                 textBoxContent.Text = "";

                 SetGridSelect();

                 refreshTasks();
            }
        }


        private void listViewUsers_MouseDown(object sender, MouseEventArgs e)
        {
         if(e.Button == System.Windows.Forms.MouseButtons.Right)
         {
             Point p = listViewUsers.PointToClient(Cursor.Position);
            contextMenuStripUsers.Show(listViewUsers, p);
          }

         if (listViewUsers.SelectedItems.Count > 0)
         {
             listViewUsers.SelectedItems[0].BackColor = Color.LightPink;
             listViewUsers.SelectedItems[0].ForeColor = Color.Purple;
         }
        }

     

        //show userGroup delegate
        //delegate void d_ShowGroupMenu();
        private void contextMenuStripUsers_Opening(object sender, CancelEventArgs e)
        {
            //Invoke(new d_ShowGroupMenu(showGroupMenu));
            addToGroupToolStripMenuItem.DropDownItems.Clear();
            Dictionary<string, UserGroup> vgs = UserGroups.ViewGroups();
            Dictionary<string, UserGroup>.ValueCollection vals = vgs.Values;
            UserGroup[] ugs = new UserGroup[vals.Count];
            vals.CopyTo(ugs,0);
            foreach(UserGroup ug in ugs)
            {
                
                ToolStripItem ts = addToGroupToolStripMenuItem.DropDownItems.Add(ug.GroupName);
                ts.Click += new EventHandler(ts_Click);
            }
            
        }

        void ts_Click(object sender, EventArgs e)
        {
            ToolStripItem ts = (ToolStripItem)sender;
            Dictionary<string, UserGroup> lin = UserGroups.ViewGroups();
            var ug =  from u in lin where u.Key == ts.Text select u; //***************
            
            foreach (var v in ug)
            {
                UserGroup ui = (UserGroup)v.Value;
                GroupUser gu = new GroupUser();


                gu.UserName = currentlySelectedUser.UserName;//
                gu.UserUniqueId = gu.UserName + currentlySelectedUser.ComputerName;
                UserGroups.AddUserToGroup(ui,gu);
            }

            refreshGroups();
        }

       

        private void userSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewUsers.SelectedItems.Count > 0)
            {
                UserSettings us = new UserSettings();
                us.Show((Lan.NetworkUser)listViewUsers.SelectedItems[0].Tag);
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            UserGroup ug = new UserGroup();
            ug.GroupName = toolStripTextBoxNewGroupName.Text ;
            UserGroups.AddGroup(ug);

            refreshGroups();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            GroupForm gf = new GroupForm();
            gf.ShowDialog();
        }

        private void treeViewGroups_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag.ToString() == "GROUP")
            {
                //we have it
                tabPage1.Text = e.Node.Text + " Messages";
                if(e.Node.Text == "Broadcast")
                    buttonSend.Text = "Broadcast To Everyone";
                else
                    buttonSend.Text = "Send To " + e.Node.Text;

                tabPage1.Tag = e.Node.Text;
                tabControl1.SelectedIndex = 0;

                //save old file here..
                //load from file here
                richMessage.Rtf = "";
                loadMessages(e.Node.Text);


                foreach (TreeNode tn in treeViewGroups.Nodes)
                {
                    tn.BackColor = Color.White;
                    tn.ForeColor = Color.Black;
                }

                e.Node.BackColor = Color.LightPink;
                e.Node.ForeColor = Color.Purple;

                foreach (ListViewItem lvi in listViewUsers.Items)
                {
                    lvi.BackColor = Color.White;
                    lvi.ForeColor = Color.Black;
                }

              
            }
            //else if (treeViewGroups.SelectedNode != null)
            //    treeViewGroups.SelectedNode.ImageIndex = 1;

        }

        private void loadMessages(string node)
        {
            richTextBoxReplies.Rtf = userMessages.LoadUserMessageText(node);
            richTextBoxReplies.Select(richTextBoxReplies.TextLength, 0);
            richTextBoxReplies.ScrollToCaret();
        }

        private void listViewUsers_MouseClick(object sender, MouseEventArgs e)
        {
            if (listViewUsers.SelectedIndices.Count > 0)
            {
                Lan.NetworkUser com = (Lan.NetworkUser)listViewUsers.SelectedItems[0].Tag;

                //if (!Lan.IsUserThere(com))
                //{
                //    Lan.NetworkUser n = currentUsers.Find(s => s.UserUniqueId == com.UserUniqueId);
                //    currentUsers.Remove(n);
                //    refreshUsers();
                //    refreshGroups();
                //}

                labelPCInfo.Text = com.UserName + " on PC: " + com.ComputerName + "\r\nIP Address:" + com.IPAddress.ToString();

                currentlySelectedUser = com;

                tabPage1.Text = com.UserName + " Messages";
                tabPage1.Tag = com;
                tabControl1.SelectedIndex = 0;

                buttonSend.Text = "Send To " + com.UserName;

                string text = userMessages.LoadUserMessageText(com.UserUniqueId);
                richTextBoxReplies.Rtf = text;
                richTextBoxReplies.Select(richTextBoxReplies.TextLength, 0);
                richTextBoxReplies.ScrollToCaret();

                richMessage.Select();

                Lan.NetworkUser nit = usersWithMessages.Find(s => s.UserUniqueId == com.UserUniqueId.ToLower());
                if(nit != null)
                    usersWithMessages.Remove(nit);
                

            }

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
        }

        private void treeViewGroups_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (treeViewGroups.SelectedNode != null)
                {

                    if(treeViewGroups.SelectedNode.Text != "Broadcast")
                    if (treeViewGroups.SelectedNode.Tag.ToString() == "GROUP")
                    {
                        Point p = treeViewGroups.PointToClient(Cursor.Position);
                        contextMenuStripGroups.Show(treeViewGroups, p);
                    }


                }

            }
            
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string groupName = treeViewGroups.SelectedNode.Tag.ToString();
            UserGroups.RemoveGroup(treeViewGroups.SelectedNode.Text);
            treeViewGroups.SelectedNode.Remove();
            
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            showSettings();
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count > 0 )
                refreshTasks(dataGridView1.SelectedRows[0].Index);

            SetGridSelect();
        }

        private void toolStripButton6_Click_2(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] fileNames = openFileDialog1.FileNames;
                //need to append these files to the message when sent 
                //and to list view or add to the filenames

                foreach (string file in fileNames)
                {
                    //Check file sizes...
                    FileInfo fi = new FileInfo(file);
                    if (fi.Length > (1048576 * Settings.Default.LargestFileSizeMB))
                    {
                        MessageBox.Show("File Too Large!", "File is Too LARGE!..", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }

                    FileObject fo = new FileObject(file);

                    bool found = false;
                    for(int i= 0; i < toolStripComboBoxAttachments.Items.Count; i++)
                    {

                        FileObject fob = (FileObject)toolStripComboBoxAttachments.Items[i];
                        if (fob.FilePath == file)
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        
                            toolStripComboBoxAttachments.Items.Add(fo);
                        //add to message
                           // richMessage.Text += "\r\nSending File: " + fo.FileName;
                    }

                }

                if(toolStripComboBoxAttachments.Items.Count > 0 )
                     toolStripComboBoxAttachments.SelectedIndex = toolStripComboBoxAttachments.Items.Count-1;
            }
        }


        private void toolStripButton7_Click_1(object sender, EventArgs e)
        {
            if (toolStripComboBoxAttachments.SelectedIndex > -1)
            {
                toolStripComboBoxAttachments.Items.Remove(toolStripComboBoxAttachments.SelectedItem);

            }

            if (toolStripComboBoxAttachments.Items.Count > 0)
                toolStripComboBoxAttachments.SelectedIndex = toolStripComboBoxAttachments.Items.Count - 1;
            else
                toolStripComboBoxAttachments.Text = "";
        }

        private void tabControl1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                refreshTasks();
            }
        }

    

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.WindowState = FormWindowState.Normal;
                this.Show();
                this.Activate();
                this.BringToFront();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSettings();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void viewFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            showUserFiles();
           
        }

        private void showUserFiles()
        {
            if (listViewUsers.SelectedItems.Count > 0)
            {
                Lan.NetworkUser nu = (Lan.NetworkUser)listViewUsers.SelectedItems[0].Tag;
                string path = Settings.Default.TransfersFolder;
                if (path == "")
                {
                    string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    path = docs + @"\NetworkChat\files";
                }

                path +=  @"\" + nu.UserName + nu.ComputerName + @"\";

                if (Directory.Exists(path)) //dont use userUniqueID))
                    Process.Start(path);
                else
                    MessageBox.Show(nu.UserName + " has not sent you any files...", nu.UserName + " has not sent you any files...", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

        }

      
        private void timerFlashUsersWhoHaveMessages_Tick(object sender, EventArgs e)
        {
            Lan.NetworkUser[] nus = new Lan.NetworkUser[usersWithMessages.Count];
            usersWithMessages.CopyTo(nus);

            foreach (Lan.NetworkUser nu in nus)
            {
                ListViewItem[] lvis = listViewUsers.Items.Find(nu.UserUniqueId,true);
                if (lvis.Length > 0)
                {
                    ListViewItem lvi = lvis[0];
                    if(lvi.BackColor == Color.Red)
                        lvi.BackColor = Color.Wheat;
                    else
                        lvi.BackColor = Color.Red;
                }
            }
        }

      

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.BringToFront();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            
           
        }

        private void listViewUsers_DoubleClick(object sender, EventArgs e)
        {
            showUserFiles();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
          
            richMessage.SelectionFont = ToggleFontStyle(richMessage.SelectionFont, FontStyle.Underline);
            toolStripButtonUnderline.Checked = richMessage.SelectionFont.Underline;
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            richMessage.SelectionFont = ToggleFontStyle(richMessage.SelectionFont, FontStyle.Strikeout);
            toolStripButtonStrikeOut.Checked = richMessage.SelectionFont.Strikeout;
        }

        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }

        private void toolStripButton5_Click_1(object sender, EventArgs e)
        {
            
            string path = Settings.Default.TransfersFolder;
            if (path == "")
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = docs + @"\NetworkChat\files\";
               // path = Application.StartupPath + @"\files\"; //dont use userUniqueID
            }


            if (Directory.Exists(path))
                Process.Start(path);
        }

        private void toolStripButton11_Click_1(object sender, EventArgs e)
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = docs + @"\NetworkChat\messages";

            if (!Directory.Exists(path))
                MessageBox.Show("You have no messages", "You have no messages", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            else
                Process.Start(path);

        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            Process.Start("http://james-jenkins.co.uk");
        }

        void DoRegistered()
        {
            if (!Registry.IsUserRegistered())
            {
                Register r = new Register();
                r.ShowDialog();

                if(!Registry.IsUserRegistered())
                    toolStripButtonRegister.Visible = true;
                else
                    toolStripButtonRegister.Visible = false;
            }
            else
            {
                toolStripButtonRegister.Visible = false;
            }
        }

        private void toolStripButtonRegister_Click(object sender, EventArgs e)
        {

            //  Process.Start("http://james-jenkins.co.uk");
            DoRegistered();
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LD7U2LARNT5QS");
        }
    }
}
