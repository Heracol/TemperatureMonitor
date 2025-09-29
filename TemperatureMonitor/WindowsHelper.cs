using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureMonitor
{
    public static class WindowsHelper
    {
        public static bool IsRunningAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();
    }
}
