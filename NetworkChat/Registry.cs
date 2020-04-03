using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Security.Permissions;

namespace NetworkChat
{
    class Registry
    {
        public static void StartWithWindows(bool start)
        {

            RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\");

            if (start)
            {
                
                rk.SetValue("Network Chat by James Jenkins", Application.StartupPath + @"\networkchat.exe");
                rk.Close();
            }
            else
            {

                string[] names = rk.GetValueNames();

                if (names.Contains<string>("Network Chat by James Jenkins"))
                    rk.DeleteValue("Network Chat by James Jenkins", true);
               

                rk.Close();
            }
        }

        public static void SetUserAuthenticated(string email, string key)
        {
            RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\NetworkChat\");
            rk.SetValue("regEmail", email );
            rk.SetValue("regKey", key);
            rk.Close();
        }

        public static bool IsUserRegistered()
        {
            RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\NetworkChat\", false);

            if (rk != null)
            {
                string em = (string)rk.GetValue("regKey");
                rk.Close();

                if (em != null)
                    return true;
            }

            return true;
        }
    }
}
