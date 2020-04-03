using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.IO;

namespace NetworkChat
{
    
    public class UserGroups
    {
        private static Dictionary<string, UserGroup> userGroups = new Dictionary<string, UserGroup>();

        public static List<GroupUser> GetGroupMembers(string GroupName)
        {
            List<GroupUser> users = new List<GroupUser>();
             if (userGroups.ContainsKey(GroupName))
                 users = userGroups[GroupName].GroupMembers.ToList();

            return users;
        }

        public static void LoadUserGroups()
        {
            //load the usergroups from disc
            BinaryFormatter bf = new BinaryFormatter();
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string fil = docs + @"\NetworkChat\" + Environment.UserName + Lan.GetLocalComputer().ComputerName + "_grps.bin";

            if (File.Exists(fil))
            {
                FileStream fs = new FileStream(fil, FileMode.Open);
                userGroups = (Dictionary<string, UserGroup>)bf.Deserialize(fs);
                fs.Close();
            }
        }

        private static void SaveUserGroups()
        {
            //serialize to disc
            BinaryFormatter bf = new BinaryFormatter();

            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string fil = docs + @"\NetworkChat\" + Environment.UserName + Lan.GetLocalComputer().ComputerName + "_grps.bin";
            FileStream fs = new FileStream(fil, FileMode.Create);
            bf.Serialize(fs, userGroups);
            fs.Close();
            
        }

        public static Dictionary<string, UserGroup> ViewGroups()
        {
            return userGroups;
        }

        public static void AddGroup(UserGroup UserGroup)
        {

            if (!userGroups.ContainsKey(UserGroup.GroupName))
            {
                userGroups.Add(UserGroup.GroupName, UserGroup);
                SaveUserGroups();
            }
        }

        public static void RemoveGroup(string GroupName)
        {
            if (userGroups.ContainsKey(GroupName))
            {
                userGroups.Remove(GroupName);
                SaveUserGroups();
            }
        }

        public static void AddUserToGroup(UserGroup UserGroup, GroupUser GroupUser)
        {
            if (userGroups.ContainsKey(UserGroup.GroupName))
            {

                List<GroupUser> gus = userGroups[UserGroup.GroupName].GroupMembers.ToList();
                GroupUser gu = gus.Find(s => s.UserUniqueId == GroupUser.UserUniqueId);

                if (gu == null)
                {
                    userGroups[UserGroup.GroupName].GroupMembers.Add(GroupUser);
                    SaveUserGroups();
                }


            }
        }

        public static void RemoveUserFromGroup(UserGroup UserGroup, GroupUser GroupUser)
        {
            if (userGroups.ContainsKey(UserGroup.GroupName))
            {
                if (userGroups[UserGroup.GroupName].GroupMembers.Contains(GroupUser))
                {
                    userGroups[UserGroup.GroupName].GroupMembers.Remove(GroupUser);
                    SaveUserGroups();
                }
            }
        }



    }

    [Serializable()]
    public class UserGroup
    {
        public UserGroup()
        {
            GroupMembers = new List<GroupUser>();
        }

        public string GroupName { get; set; }
        public List<GroupUser> GroupMembers { get; set; }
    }

    [Serializable()]
    public class GroupUser
    {
        public string UserName { get; set; }
        public string UserUniqueId { get; set; }
        
    }
}
