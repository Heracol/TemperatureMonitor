using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TemperatureMonitor
{
    public static class AutoStart
    {
        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "TemperatureMonitor";

        public static void SetAutoStart(bool enable)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (enable)
                    key.SetValue(AppName, Application.ExecutablePath);
                else
                    key.DeleteValue(AppName, false);
            }
        }

        public static bool IsAutoStartEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
            {
                return key?.GetValue(AppName) != null;
            }
        }
    }
}
