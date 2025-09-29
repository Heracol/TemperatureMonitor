using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TemperatureMonitor
{
    public class MenuBuilder
    {
        private readonly Settings settings;
        private readonly HardwareMonitor hardwareMonitor;
        private readonly System.Windows.Forms.Timer timer;
        private readonly Action update;

        public MenuBuilder(Settings settings, HardwareMonitor hardwareMonitor, System.Windows.Forms.Timer timer, Action update)
        {
            this.settings = settings;
            this.hardwareMonitor = hardwareMonitor;
            this.timer = timer;
            this.update = update;
        }

        public ContextMenuStrip BuildCpuMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add(BuildGpuToggleItem());
            menu.Items.Add(BuildAutoStartItem());
            menu.Items.Add(BuildTemperatureUnitMenu());
            menu.Items.Add(BuildUpdateIntervalMenu());
            menu.Items.Add(BuildVisualsMenu());
            menu.Items.Add(BuildExitItem());

            return menu;
        }

        public ContextMenuStrip BuildGpuMenu()
        {
            var menu = new ContextMenuStrip();

            (ToolStripMenuItem bgColorMenu, ToolStripMenuItem textColorMenu) = BuildGpuColorMenu();

            menu.Items.Add(bgColorMenu);
            menu.Items.Add(textColorMenu);

            return menu;
        }

        private ToolStripMenuItem BuildGpuToggleItem()
        {
            var gpuItem = new ToolStripMenuItem("Show GPU")
            {
                Enabled = hardwareMonitor.HasGpu,
                CheckOnClick = true,
                Checked = settings.ShowGpu
            };

            gpuItem.CheckedChanged += (_, __) =>
            {
                settings.ShowGpu = gpuItem.Checked;
                update.Invoke();
            };

            return gpuItem;
        }

        private ToolStripMenuItem BuildAutoStartItem()
        {
            var autoStartItem = new ToolStripMenuItem("Start with Windows")
            {
                CheckOnClick = true,
                Checked = AutoStart.IsAutoStartEnabled()
            };

            autoStartItem.CheckedChanged += (_, __) =>
            {
                AutoStart.SetAutoStart(autoStartItem.Checked);
            };

            return autoStartItem;
        }

        private ToolStripMenuItem BuildTemperatureUnitMenu()
        {
            var unitMenu = new ToolStripMenuItem("Temperature Unit");

            var celsiusItem = new ToolStripMenuItem("Celsius") { CheckOnClick = true, Checked = !settings.InFahrenheit };
            var fahrenheitItem = new ToolStripMenuItem("Fahrenheit") { CheckOnClick = true, Checked = settings.InFahrenheit };

            celsiusItem.Click += (_, __) =>
            {
                settings.InFahrenheit = false;

                celsiusItem.Checked = true;
                fahrenheitItem.Checked = false;

                update.Invoke();
            };

            fahrenheitItem.Click += (_, __) =>
            {
                settings.InFahrenheit = true;

                celsiusItem.Checked = false;
                fahrenheitItem.Checked = true;

                update.Invoke();
            };

            unitMenu.DropDownItems.Add(celsiusItem);
            unitMenu.DropDownItems.Add(fahrenheitItem);

            return unitMenu;
        }

        private ToolStripMenuItem BuildUpdateIntervalMenu()
        {
            var intervalMenu = new ToolStripMenuItem("Update Interval");
            int[] intervals = { 250, 1000, 3000, 5000, 10000 };

            foreach (var ms in intervals)
            {
                string label = ms < 1000 ? $"{ms} ms" : $"{ms / 1000} s";
                var item = new ToolStripMenuItem(label) { CheckOnClick = true, Checked = ms == settings.UpdateInterval };

                item.Click += (_, __) =>
                {
                    foreach (ToolStripMenuItem sibling in intervalMenu.DropDownItems)
                        sibling.Checked = false;

                    item.Checked = true;
                    settings.UpdateInterval = ms;

                    timer.Interval = ms;
                };

                intervalMenu.DropDownItems.Add(item);
            }

            return intervalMenu;
        }

        private ToolStripMenuItem BuildVisualsMenu()
        {
            var visualsMenu = new ToolStripMenuItem("Visuals");

            visualsMenu.DropDownItems.Add(BuildFontSizeMenu());
            visualsMenu.DropDownItems.Add(BuildShowDegreeSymbolItem());

            (ToolStripMenuItem bgColorMenu, ToolStripMenuItem textColorMenu) = BuildCpuColorMenu();

            visualsMenu.DropDownItems.Add(bgColorMenu);
            visualsMenu.DropDownItems.Add(textColorMenu);

            return visualsMenu;
        }

        private ToolStripMenuItem BuildFontSizeMenu()
        {
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

            largeItem.Click += (_, __) => UpdateFontSize(FontSize.Large, largeItem, mediumItem, smallItem);
            mediumItem.Click += (_, __) => UpdateFontSize(FontSize.Medium, largeItem, mediumItem, smallItem);
            smallItem.Click += (_, __) => UpdateFontSize(FontSize.Small, largeItem, mediumItem, smallItem);

            fontMenu.DropDownItems.Add(largeItem);
            fontMenu.DropDownItems.Add(mediumItem);
            fontMenu.DropDownItems.Add(smallItem);

            return fontMenu;
        }

        private void UpdateFontSize(FontSize size, params ToolStripMenuItem[] items)
        {
            settings.FontSizeValue = size;

            foreach (var item in items)
                item.Checked = (item.Text == size.ToString());

            update.Invoke();
        }

        private ToolStripMenuItem BuildShowDegreeSymbolItem()
        {
            var showDegreeItem = new ToolStripMenuItem("Show Degree Symbol")
            {
                CheckOnClick = true,
                Checked = settings.ShowDegreeSymbol
            };

            showDegreeItem.CheckedChanged += (_, __) =>
            {
                settings.ShowDegreeSymbol = showDegreeItem.Checked;
                update.Invoke();
            };

            return showDegreeItem;
        }

        private ToolStripMenuItem BuildExitItem()
        {
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (_, __) => Application.Exit();
            return exitItem;
        }

        public (ToolStripMenuItem bg, ToolStripMenuItem fg) BuildCpuColorMenu()
        {
            var bgColors = new Dictionary<string, Color?>
            {
                { "Transparent", Color.Transparent },
                { "Dark Blue", Color.DarkBlue },
                { "Custom...", null }
            };

            var fgColors = new Dictionary<string, Color?>
            {
                { "White", Color.White },
                { "Black", Color.Black },
                { "Light Sky Blue", Color.LightSkyBlue },
                { "Custom...", null }
            };

            var bgMenu = BuildColorChoiceMenu(
                "Background Color",
                settings.CpuBackgroundColor,
                bgColors,
                c => settings.CpuBackgroundColor = c
            );

            var fgMenu = BuildColorChoiceMenu(
                "Text Color",
                settings.CpuTextColor,
                fgColors,
                c => settings.CpuTextColor = c
            );

            return (bgMenu, fgMenu);
        }

        public (ToolStripMenuItem bg, ToolStripMenuItem fg) BuildGpuColorMenu()
        {
            var bgColors = new Dictionary<string, Color?>
            {
                { "Transparent", Color.Transparent },
                { "Dark Green", Color.DarkGreen },
                { "Custom...", null }
            };

            var fgColors = new Dictionary<string, Color?>
            {
                { "White", Color.White },
                { "Dark Green", Color.DarkGreen },
                { "Light Green", Color.LightGreen },
                { "Custom...", null }
            };

            var bgMenu = BuildColorChoiceMenu(
                "Background Color",
                settings.GpuBackgroundColor,
                bgColors,
                c => settings.GpuBackgroundColor = c
            );

            var fgMenu = BuildColorChoiceMenu(
                "Text Color",
                settings.GpuTextColor,
                fgColors,
                c => settings.GpuTextColor = c
            );

            return (bgMenu, fgMenu);
        }

        private ToolStripMenuItem BuildColorChoiceMenu(
            string title,
            Color currentColor,
            Dictionary<string, Color?> colors,
            Action<Color> setColor)
        {
            var menu = new ToolStripMenuItem(title);
            bool matched = false;

            foreach (var kvp in colors)
            {
                string label = kvp.Key;
                Color? color = kvp.Value;

                var item = new ToolStripMenuItem(label) { CheckOnClick = true };

                if (color.HasValue && color.Value.ToArgb() == currentColor.ToArgb())
                {
                    item.Checked = true;
                    matched = true;
                }

                if (!color.HasValue && !matched)
                {
                    item.Checked = true;
                }

                item.Click += (_, __) =>
                {
                    foreach (ToolStripMenuItem sibling in menu.DropDownItems)
                        sibling.Checked = false;

                    item.Checked = true;

                    if (!color.HasValue)
                    {
                        using (var dlg = new ColorDialog { FullOpen = true, Color = currentColor })
                        {
                            if (dlg.ShowDialog() == DialogResult.OK)
                                setColor(dlg.Color);
                        }
                    }
                    else
                    {
                        setColor(color.Value);
                    }

                    update.Invoke();
                };

                menu.DropDownItems.Add(item);
            }

            return menu;
        }
    }
}
