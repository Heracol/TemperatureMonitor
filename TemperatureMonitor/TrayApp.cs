using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TemperatureMonitor
{
    public class TrayApp
    {
        private readonly NotifyIcon cpuIcon = new NotifyIcon();
        private readonly NotifyIcon gpuIcon = new NotifyIcon();

        private readonly HardwareMonitor hardwareMonitor;

        private readonly Settings settings;

        private Timer timer;

        public TrayApp(HardwareMonitor hardwareMonitor, Settings settings)
        {
            this.hardwareMonitor = hardwareMonitor;
            this.settings = settings;

            timer = new Timer();
            timer.Interval = settings.UpdateInterval;
            timer.Tick += (s, e) => UpdateTemperature();

            cpuIcon.Text = "CPU Temperature";
            gpuIcon.Text = "GPU Temperature";

            cpuIcon.Visible = true;
            gpuIcon.Visible = true;

            MenuBuilder menuBuilder = new MenuBuilder(settings, hardwareMonitor, timer, UpdateTemperature);

            cpuIcon.ContextMenuStrip = menuBuilder.BuildCpuMenu();
            gpuIcon.ContextMenuStrip = menuBuilder.BuildGpuMenu();

            Application.ApplicationExit += (s, e) => Close();
        }

        public void Start()
        {
            timer.Start();
            Application.Run();
        }

        private void UpdateTemperature()
        {
            UpdateIcon(cpuIcon, hardwareMonitor.GetCpuTemperature(), settings.CpuBackgroundColor, settings.CpuTextColor, settings.FontSizeValue, settings.ShowDegreeSymbol);

            if (settings.ShowGpu && hardwareMonitor.HasGpu)
                UpdateIcon(gpuIcon, hardwareMonitor.GetGpuTemperature(), settings.GpuBackgroundColor, settings.GpuTextColor, settings.FontSizeValue, settings.ShowDegreeSymbol);
            else
                gpuIcon.Icon = null;
        }

        private void UpdateIcon(NotifyIcon notifyIcon, float? temperature, Color background, Color foreground, FontSize fontSize, bool addDegreeSymbol)
        {
            if (temperature.HasValue)
            {
                int value = (int)Math.Round(settings.InFahrenheit ? temperature.Value * 1.8f + 32 : temperature.Value);

                Icon icon = IconGenerator.CreateIcon(value, background, foreground, fontSize, addDegreeSymbol);

                notifyIcon.Icon = icon;

                IconGenerator.DestroyIcon(icon.Handle);
            }
        }

        public void Close()
        {
            timer.Stop();
            timer.Dispose();
            
            hardwareMonitor.Close();

            cpuIcon.Dispose();
            gpuIcon.Dispose();
        }
    }
}
