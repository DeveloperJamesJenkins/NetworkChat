using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NetworkChat.Properties;

namespace NetworkChat
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                    MessageBox.Show(args[0]);

                if (args.Length > 0)
                {
                    if (args[0] == "reset")
                    {

                        bool reg = Settings.Default.Registered;
                        Settings.Default.Reset();
                        Settings.Default.Registered = reg;
                        Settings.Default.Save();
                        Settings.Default.Reload();
                        MessageBox.Show("Settings have been reset", "Settings have been reset", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
    }
}
