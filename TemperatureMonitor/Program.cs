using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace TemperatureMonitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            SetProcessDPIAware();

            Settings settings = new Settings();
            settings.Load();

            HardwareMonitor monitor = new HardwareMonitor();

            TrayApp app = new TrayApp(monitor, settings);

            app.Start();
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
