using System;
using System.Runtime.InteropServices;
using TemperatureMonitor;

WindowsHelper.SetProcessDPIAware();

if (!WindowsHelper.IsRunningAdmin())
{
    MessageBox.Show(
        "TemperatureMonitor needs to be run as Administrator.",
        "Administrator Required",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning
    );
    return;
}

Settings settings = Settings.Load();

HardwareMonitor monitor = new HardwareMonitor();

TrayApp app = new TrayApp(monitor, settings);

app.Start();
