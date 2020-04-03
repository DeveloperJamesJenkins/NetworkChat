using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using NetworkChat.Properties;
using System.Windows.Forms;

namespace NetworkChat
{
    class TasksLocalShare
    {
        static string fil = "";//@"\\SEAN-PC\SharedDocs\Tasks.xml.txt";
        public static List<Task> CurrentTasks { get; private set; }

        private static string GetTaskFileLocation()
        {
            string s = Settings.Default.TaskFolder + @"\Tasks.xml.txt";
           
            return s;
        }

        public static void CreateTaskFile(string Path)
        {
            Path +=  @"\Tasks.xml.txt";
            if (!File.Exists(Path))
            {
                try
                {
                   // System.Security.AccessControl.FileSecurity acs = new System.Security.AccessControl.FileSecurity();
                    
                    FileStream fs;
                    fs = new FileStream(Path, FileMode.Create, FileAccess.Write);// File.Create(Path);//,0, FileOptions.None, acs);
                    string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Tasks></Tasks>";
                    xml = XmlEncryption.encryptXml(xml);
                    byte[] bits = ASCIIEncoding.ASCII.GetBytes(xml);
                    fs.Write(bits, 0, bits.Length);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public static List<Task> LoadTaskList()
        {
            fil = GetTaskFileLocation();
            List<Task> tasks = new List<Task>();
            tasks = getTasksFromXml();
            CurrentTasks = tasks;
            return tasks;
        }

        private static List<Task> getTasksFromXml()
        {

            List<Task> tasks = new List<Task>();

            try
            {

                if (!File.Exists(fil))
                    return null;

                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(fil);
                XmlDocument xx = new XmlDocument();
                xx.LoadXml(XmlEncryption.decryptXml(xdoc.InnerXml));

                XmlNodeList tasksNodes = xx.SelectNodes("Tasks/Task");

                if (tasksNodes != null)
                {
                    for(int i =0 ; i < tasksNodes.Count; i++)
                    {
                        XmlNode task = tasksNodes[i];

                        Task t = new Task();
                        t.LastEditBy = task.SelectSingleNode("LastEditBy").InnerText;
                        t.LastEditDate = DateTime.Parse(task.SelectSingleNode("LastEditDate").InnerText);
                        t.TaskDetails = task.SelectSingleNode("TaskDetails").InnerText;
                        t.TaskId = task.Attributes["TaskId"].InnerText;
                        t.TaskStatus = (TaskStatus)Enum.Parse(typeof(TaskStatus), task.SelectSingleNode("TaskStatus").InnerText);
                        t.TaskTitle = task.SelectSingleNode("TaskTitle").InnerText;
                        tasks.Add(t);

                    }
                }
            }
            catch(Exception ex)
            {
                return null;
            }

            return tasks;
        }


        private static int getNewId()
        {
            var max = (from item in getTasksFromXml() select item.TaskId).Max();

            if (max == null)
                return 0;
            else
                return int.Parse(max)+1;
        }

        public static void DeleteTask(string Id)
        {
            XmlDocument xdocEn = new XmlDocument();
            xdocEn.Load(fil);

            XmlDocument xdoc = new XmlDocument();
            string decRypted = XmlEncryption.decryptXml(xdocEn.InnerXml);
            xdoc.LoadXml(decRypted);

            XmlNode task = xdoc.SelectSingleNode("Tasks/Task[@TaskId='" + Id + "']");
            xdoc.DocumentElement.RemoveChild(task);

            xdoc.InnerXml = XmlEncryption.encryptXml(xdoc.InnerXml);
            xdoc.Save(fil);

        }

        public static void UpdateTask(Task Task)
        {
            XmlDocument xdocEn = new XmlDocument();
            xdocEn.Load(fil);

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(XmlEncryption.decryptXml(xdocEn.InnerXml));

            XmlNode task = xdoc.SelectSingleNode("Tasks/Task[@TaskId='" + Task.TaskId + "']");
            task.SelectSingleNode("TaskDetails").InnerText = Task.TaskDetails;
            task.SelectSingleNode("LastEditBy").InnerText = Task.LastEditBy;
            task.SelectSingleNode("LastEditDate").InnerText = Task.LastEditDate.ToString();
            task.SelectSingleNode("TaskStatus").InnerText = Enum.GetName(typeof(TasksLocalShare.TaskStatus), Task.TaskStatus);

            xdoc.InnerXml = XmlEncryption.encryptXml(xdoc.InnerXml);
            xdoc.Save(fil);

        }


        //adds task to xml file...
        public static void AddTask(Task Task)
        {
            try
            {
                XmlDocument xdocEn = new XmlDocument();
                xdocEn.Load(fil);

                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(XmlEncryption.decryptXml(xdocEn.InnerXml));


                XmlNode tasksNode = xdoc.SelectSingleNode("Tasks");

                XmlElement taskNode = xdoc.CreateElement("Task");


                XmlAttribute taskTaskId = xdoc.CreateAttribute("TaskId");
                taskTaskId.InnerText = getNewId().ToString();
                taskNode.Attributes.Append(taskTaskId);

                XmlElement taskTitleNode = xdoc.CreateElement("TaskTitle");
                taskTitleNode.InnerText = Task.TaskTitle;
                taskNode.AppendChild(taskTitleNode);

                XmlElement taskTaskDetails = xdoc.CreateElement("TaskDetails");
                taskTaskDetails.InnerText = Task.TaskDetails;
                taskNode.AppendChild(taskTaskDetails);

                XmlElement taskTaskStatus = xdoc.CreateElement("TaskStatus");
                taskTaskStatus.InnerText = Enum.GetName(typeof(TaskStatus), Task.TaskStatus);
                taskNode.AppendChild(taskTaskStatus);

                XmlElement taskLastEditDate = xdoc.CreateElement("LastEditDate");
                taskLastEditDate.InnerText = Task.LastEditDate.ToString();
                taskNode.AppendChild(taskLastEditDate);

                XmlElement taskLastEditBy = xdoc.CreateElement("LastEditBy");
                taskLastEditBy.InnerText = Task.LastEditBy;
                taskNode.AppendChild(taskLastEditBy);

                tasksNode.AppendChild(taskNode);

                xdoc.InnerXml = XmlEncryption.encryptXml(xdoc.InnerXml);

                xdoc.Save(fil);
            }
            catch (Exception ex)
            {
            }

        }



        public class Task
        {
            public string TaskId { get; set; }
            public string TaskTitle { get; set; }
            public string TaskDetails { get; set; }
            public TaskStatus TaskStatus { get; set; }
            public DateTime LastEditDate { get; set; }
            public string LastEditBy { get; set; }
        }

        public enum TaskStatus
        {
            NotStarted,
            Started,
            Complete
        }
    }
}
