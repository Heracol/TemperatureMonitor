using System.Runtime.InteropServices;
using System.Security.Principal;

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
