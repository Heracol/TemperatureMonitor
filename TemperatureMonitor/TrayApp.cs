using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
            gpuIcon.ContextMenuStrip = BuildGpuMenu();

            Application.ApplicationExit += (s, e) => Close();
        }

        public void Start()
        {
            timer = new System.Threading.Timer(UpdateTemperature, null, 0, settings.UpdateInterval);
            Application.Run();
        }

        private void UpdateTemperature(object state)
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

        public ContextMenuStrip BuildCpuMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            // ===== Show GPU =====
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

            // ===== Temperature Unit =====
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

            // ===== Update Interval =====
            var intervalMenu = new ToolStripMenuItem("Update Interval");
            int[] intervals = { 250, 1000, 3000, 5000, 10000 };

            foreach (var ms in intervals)
            {
                string label = ms < 1000 ? $"{ms} ms" : $"{ms / 1000} s";
                var item = new ToolStripMenuItem(label) { CheckOnClick = true };
                if (ms == settings.UpdateInterval) item.Checked = true;

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

            // ===== Options / Visuals =====
            var visualsMenu = new ToolStripMenuItem("Visuals");

            // --- Font Size ---
            var fontMenu = new ToolStripMenuItem("Font Size");
            var largeItem = new ToolStripMenuItem("Large") { CheckOnClick = true };
            var mediumItem = new ToolStripMenuItem("Medium") { CheckOnClick = true };
            var smallItem = new ToolStripMenuItem("Small") { CheckOnClick = true };

            switch (settings.FontSizeValue)
            {
                case FontSize.Large: largeItem.Checked = true; break;
                case FontSize.Medium: mediumItem.Checked = true; break;
                case FontSize.Small: smallItem.Checked = true; break;
            }

            largeItem.Click += (s, e) => { settings.FontSizeValue = FontSize.Large; largeItem.Checked = true; mediumItem.Checked = false; smallItem.Checked = false; };
            mediumItem.Click += (s, e) => { settings.FontSizeValue = FontSize.Medium; largeItem.Checked = false; mediumItem.Checked = true; smallItem.Checked = false; };
            smallItem.Click += (s, e) => { settings.FontSizeValue = FontSize.Small; largeItem.Checked = false; mediumItem.Checked = false; smallItem.Checked = true; };

            fontMenu.DropDownItems.Add(largeItem);
            fontMenu.DropDownItems.Add(mediumItem);
            fontMenu.DropDownItems.Add(smallItem);
            visualsMenu.DropDownItems.Add(fontMenu);

            // --- Show Degree Symbol ---
            var showDegreeItem = new ToolStripMenuItem("Show Degree Symbol") { CheckOnClick = true, Checked = settings.ShowDegreeSymbol };
            showDegreeItem.CheckedChanged += (s, e) => { settings.ShowDegreeSymbol = showDegreeItem.Checked; };
            visualsMenu.DropDownItems.Add(showDegreeItem);

            (ToolStripMenuItem bgColorMenu, ToolStripMenuItem textColorMenu) = BuildColorMenu(
                settings.CpuBackgroundColor, 
                settings.CpuTextColor, 
                c => settings.CpuBackgroundColor = c, 
                c => settings.CpuTextColor = c);

            // Add to your main menu or Visuals submenu
            visualsMenu.DropDownItems.Add(bgColorMenu);
            visualsMenu.DropDownItems.Add(textColorMenu);

            menu.Items.Add(visualsMenu);

            // ===== Exit =====
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => { Application.Exit(); };
            menu.Items.Add(exitItem);

            return menu;
        }

        public ContextMenuStrip BuildGpuMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            (ToolStripMenuItem bgColorMenu, ToolStripMenuItem textColorMenu) = BuildColorMenu(
                settings.GpuBackgroundColor,
                settings.GpuTextColor,
                c => settings.GpuBackgroundColor = c,
                c => settings.GpuTextColor = c);

            menu.Items.Add(bgColorMenu);
            menu.Items.Add(textColorMenu);

            return menu;
        }

        public (ToolStripMenuItem bg, ToolStripMenuItem fg) BuildColorMenu(Color settingsBgColor, Color settingsFgColor, Action<Color> setBg, Action<Color> setFg)
        {
            // --- Background Color ---
            var bgColorMenu = new ToolStripMenuItem("Background Color");

            string customText = "Custom...";
            string[] bgPresets = { "Transparent", "Dark Blue", customText };
            bool isCustom = true;

            foreach (var c in bgPresets)
            {
                var item = new ToolStripMenuItem(c);

                if (c != customText && Color.FromName(c.Replace(" ", "")).ToArgb() == settingsBgColor.ToArgb())
                {
                    isCustom = false;
                    item.Checked = true;
                }

                if (c == customText && isCustom)
                {
                    item.Checked = true;
                }

                item.Click += (s, e) =>
                {
                    if (c == customText)
                    {
                        using (var dlg = new ColorDialog())
                        {
                            dlg.FullOpen = true;
                            dlg.Color = settingsBgColor;
                            if (dlg.ShowDialog() == DialogResult.OK)
                                setBg(dlg.Color);
                        }
                    }
                    else
                    {
                        setBg(Color.FromName(c.Replace(" ", "")));
                    }
                };

                bgColorMenu.DropDownItems.Add(item);
            }

            // --- Text Color ---
            var textColorMenu = new ToolStripMenuItem("Text Color");
            string[] textPresets = { "White", customText };
            isCustom = true;

            foreach (var c in textPresets)
            {
                var item = new ToolStripMenuItem(c) { CheckOnClick = true };

                if (c != customText && Color.FromName(c.Replace(" ", "")).ToArgb() == settingsFgColor.ToArgb())
                {
                    isCustom = false;
                    item.Checked = true;
                }

                if (c == customText && isCustom)
                {
                    item.Checked = true;
                }

                item.Click += (s, e) =>
                {
                    foreach (ToolStripMenuItem sibling in textColorMenu.DropDownItems)
                        sibling.Checked = false;
                    item.Checked = true;

                    if (c == customText)
                    {
                        using (var dlg = new ColorDialog())
                        {
                            dlg.FullOpen = true;
                            dlg.Color = settingsFgColor;
                            if (dlg.ShowDialog() == DialogResult.OK)
                                setFg(dlg.Color);
                        }
                    }
                    else
                    {
                        setFg(Color.FromName(c.Replace(" ", "")));
                    }
                };

                textColorMenu.DropDownItems.Add(item);
            }

            return (bgColorMenu, textColorMenu);
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
