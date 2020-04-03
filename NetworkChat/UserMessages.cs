using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NetworkChat.Properties;

namespace NetworkChat
{
    class UserMessages
    {
        
        public UserMessages()
        {
            if (Settings.Default.LogsFolder == "")
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                Settings.Default.LogsFolder = docs + @"\NetworkChat\messages";
                MainSettings.saveSettings();
            }

            string path = Settings.Default.LogsFolder;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public string LoadUserMessageText(string UserUniqueID)
        {
            string path = Settings.Default.LogsFolder;
            if (path == "")
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = docs + @"\NetworkChat\messages";

            }
            path += @"\" + UserUniqueID.ToLower() + ".rtf";

            if (!File.Exists(path))
                return "";

            return File.ReadAllText(path);
         
        }


        public void SaveUserMessageText(string UserUniqueID, string RichText)
        {

            string path = Settings.Default.LogsFolder;
            if (path == "")
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = docs + @"\NetworkChat\messages";

            }
            
            path += @"\" + UserUniqueID.ToLower() + ".rtf";
                if (!File.Exists(path))
                {
                    FileStream fs = File.Create(path);
                    fs.Close();
                }


                File.WriteAllText(path, RichText);
            
        }

        public static void DeleteAllMessages()
        {
            string path = Settings.Default.LogsFolder;
            if (path == "")
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = docs + @"\NetworkChat\messages\";

            }

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }

        }

        public static void MoveFiles(string OldPath)
        {
            string[] files = Directory.GetFiles(OldPath);
            string path = Settings.Default.LogsFolder;
            if (path == "")
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = docs + @"\NetworkChat\messages";

            }
            foreach (string file in files)
            {
                string filName = new FileInfo(file).Name;
                File.Copy(file, path + @"\" + filName);
            }

        }
    }
}
