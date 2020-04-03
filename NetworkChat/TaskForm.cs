using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetworkChat
{
    public partial class TaskForm : Form
    {
        public TaskForm()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void TaskForm_Load(object sender, EventArgs e)
        {
            string[] names = Enum.GetNames(typeof(TasksLocalShare.TaskStatus));
            foreach (string name in names)
            {
                comboBoxStatus.Items.Add(Enum.Parse(typeof(TasksLocalShare.TaskStatus), name));
            }

            comboBoxStatus.SelectedIndex = 0;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            TasksLocalShare.Task task = new TasksLocalShare.Task();
            //dont need id on save... ***********************
            task.LastEditBy = Environment.UserName;
            task.LastEditDate = DateTime.Now;
            task.TaskDetails = textBoxNotes.Text;
            task.TaskStatus = (TasksLocalShare.TaskStatus)comboBoxStatus.SelectedItem;
            task.TaskTitle = textBoxTitle.Text;
            TasksLocalShare.AddTask(task);

            this.Close();
        }
    }
}
