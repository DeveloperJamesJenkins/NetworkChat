using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace NetworkChat
{
    class Class1 : RichTextBox
    {
        //protected override void WndProc(ref Message m)
        //{
        //    if (m.ToString().IndexOf("WM_REFLECT + WM_COMMAND") > 0)
        //    {
        //        this.SelectionBackColor = Color.Green;
        //        return;
        //    }
        //    else
        //    {
        //        Console.WriteLine(m.ToString());
        //        base.WndProc(ref m);
        //    }

            
        //}

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Class1
            // 
            this.EnableAutoDragDrop = true;
            this.ResumeLayout(false);

        }
    }
}
