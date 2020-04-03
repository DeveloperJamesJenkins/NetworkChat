using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NetworkChat
{
    class ScreenSaver
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uAction, int uParam, ref int lpvParam, int fuWinIni);
        const int SPI_GETSCREENSAVERRUNNING = 114;
        static int screenSaverRunning = -1;

        public static bool IsScreenSaverOn()
        {

            int ok = SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref screenSaverRunning, 0);
            if (screenSaverRunning != 0)
                return true;

            return false;

        }
    }
}
