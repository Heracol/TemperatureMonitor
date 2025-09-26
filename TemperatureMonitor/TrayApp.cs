using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TemperatureMonitor
{
    public class TrayApp
    {
        private readonly NotifyIcon cpuIcon = new NotifyIcon();
        private readonly NotifyIcon gpuIcon = new NotifyIcon();

        private readonly HardwareMonitor hardwareMonitor;

        private readonly Settings settings;

        private System.Threading.Timer timer;

        public TrayApp(HardwareMonitor hardwareMonitor, Settings settings)
        {
            this.hardwareMonitor = hardwareMonitor;
            this.settings = settings;

            cpuIcon.Text = "CPU Temperature";
            gpuIcon.Text = "GPU Temperature";

            cpuIcon.Visible = true;
            gpuIcon.Visible = true;

            cpuIcon.ContextMenuStrip = BuildCpuMenu();

            Application.ApplicationExit += (s, e) => Close();
        }

        public void Start()
        {
            timer = new System.Threading.Timer(UpdateTemperature, null, 0, settings.UpdateInterval);
            Application.Run();
        }

        private void UpdateTemperature(object state)
        {
            UpdateIcon(cpuIcon, hardwareMonitor.GetCpuTemperature(), Color.Transparent, Color.White, settings.FontSizeValue, settings.ShowDegreeSymbol);

            if (settings.ShowGpu && hardwareMonitor.HasGpu)
                UpdateIcon(gpuIcon, hardwareMonitor.GetGpuTemperature(), Color.Transparent, Color.LightGreen, settings.FontSizeValue, settings.ShowDegreeSymbol);
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

        public ContextMenuStrip BuildCpuMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            // Show GPU
            var gpuItem = new ToolStripMenuItem("Show GPU")
            {
                Enabled = hardwareMonitor.HasGpu,
                CheckOnClick = true,
                Checked = settings.ShowGpu
            };

            gpuItem.CheckedChanged += (s, e) =>
            {
                settings.ShowGpu = gpuItem.Checked;
            };
            
            menu.Items.Add(gpuItem);

            // Temperature Unit
            var unitMenu = new ToolStripMenuItem("Temperature Unit");
            var celsiusItem = new ToolStripMenuItem("Celsius") { CheckOnClick = true, Checked = !settings.InFahrenheit };
            var fahrenheitItem = new ToolStripMenuItem("Fahrenheit") { CheckOnClick = true, Checked = settings.InFahrenheit };

            celsiusItem.Click += (s, e) =>
            {
                settings.InFahrenheit = false;

                celsiusItem.Checked = true;
                fahrenheitItem.Checked = false;
            };

            fahrenheitItem.Click += (s, e) =>
            {
                settings.InFahrenheit = true;

                celsiusItem.Checked = false;
                fahrenheitItem.Checked = true;
            };

            unitMenu.DropDownItems.Add(celsiusItem);
            unitMenu.DropDownItems.Add(fahrenheitItem);

            menu.Items.Add(unitMenu);

            // Update Interval
            var intervalMenu = new ToolStripMenuItem("Update Interval");

            int[] intervals = { 250, 1000, 3000, 5000, 10000 };

            foreach (var ms in intervals)
            {
                string label = ms < 1000 ? $"{ms} ms" : $"{ms / 1000} s";
                var item = new ToolStripMenuItem(label) { CheckOnClick = true };

                if (ms == settings.UpdateInterval)
                    item.Checked = true;

                item.Click += (s, e) =>
                {
                    foreach (ToolStripMenuItem sibling in intervalMenu.DropDownItems)
                        sibling.Checked = false;

                    item.Checked = true;

                    settings.UpdateInterval = ms;

                    timer?.Change(0, ms);
                };

                intervalMenu.DropDownItems.Add(item);
            }

            menu.Items.Add(intervalMenu);

            // Font Size
            var fontMenu = new ToolStripMenuItem("Font Size");

            var largeItem = new ToolStripMenuItem("Large") { CheckOnClick = true };
            var mediumItem = new ToolStripMenuItem("Medium") { CheckOnClick = true };
            var smallItem = new ToolStripMenuItem("Small") { CheckOnClick = true };

            switch (settings.FontSizeValue)
            {
                case FontSize.Large:
                    largeItem.Checked = true;
                    break;
                case FontSize.Medium:
                    mediumItem.Checked = true;
                    break;
                case FontSize.Small:
                    smallItem.Checked = true;
                    break;
            }

            largeItem.Click += (s, e) =>
            {
                settings.FontSizeValue = FontSize.Large;

                largeItem.Checked = true;
                mediumItem.Checked = false;
                smallItem.Checked = false;
            };

            mediumItem.Click += (s, e) =>
            {
                settings.FontSizeValue = FontSize.Medium;

                largeItem.Checked = false;
                mediumItem.Checked = true;
                smallItem.Checked = false;
            };

            smallItem.Click += (s, e) =>
            {
                settings.FontSizeValue = FontSize.Small;

                largeItem.Checked = false;
                mediumItem.Checked = false;
                smallItem.Checked = true;
            };

            fontMenu.DropDownItems.Add(largeItem);
            fontMenu.DropDownItems.Add(mediumItem);
            fontMenu.DropDownItems.Add(smallItem);

            menu.Items.Add(fontMenu);

            // Show Degree Symbol
            var showDegreeSymbol = new ToolStripMenuItem("Show Degree Symbol")
            {
                CheckOnClick = true,
                Checked = settings.ShowDegreeSymbol
            };

            showDegreeSymbol.CheckedChanged += (s, e) =>
            {
                settings.ShowDegreeSymbol = showDegreeSymbol.Checked;
            };

            menu.Items.Add(showDegreeSymbol);

            // Exit
            var exitItem = new ToolStripMenuItem("Exit");

            exitItem.Click += (s, e) => 
            { 
                Application.Exit(); 
            };

            menu.Items.Add(exitItem);

            return menu;
        }

        public void Close()
        {
            timer?.Dispose();
            
            hardwareMonitor.Close();

            cpuIcon.Dispose();
            gpuIcon.Dispose();
        }
    }
}
