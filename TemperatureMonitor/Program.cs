using System;
using System.Runtime.InteropServices;
using TemperatureMonitor;

SetProcessDPIAware();

Settings settings = Settings.Load();

HardwareMonitor monitor = new HardwareMonitor();

TrayApp app = new TrayApp(monitor, settings);

app.Start();

[DllImport("user32.dll")]
static extern bool SetProcessDPIAware();
