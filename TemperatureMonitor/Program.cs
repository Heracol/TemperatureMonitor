using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TemperatureMonitor
{
    internal class Program
    {
        static Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true
        };

        static NotifyIcon notifyIcon = new NotifyIcon();
        static NotifyIcon notifyIconG = new NotifyIcon();

        static bool inFahrenheit = false;
        static int updateInterval = 1000;
        static IHardware gpu;
        static bool hasGPU = false;

        static bool showGPU = false;

        static System.Threading.Timer temperatureTimer;

        static void Main(string[] args)
        {
            SetProcessDPIAware();

            StartProcess();

            notifyIcon.ContextMenuStrip = GetMenu();
            notifyIcon.Text = "CPU Temperature";
            notifyIcon.Visible = true;

            notifyIconG.Text = "GPU Temperature";
            notifyIconG.Visible = true;

            Application.ApplicationExit += new EventHandler(Close);

            Application.Run();
        }

        public static void Close(object sender, EventArgs e)
        {
            computer.Close();

            notifyIcon.Icon.Dispose();
            notifyIcon.Dispose();

            notifyIconG.Icon.Dispose();
            notifyIconG.Dispose();
        }

        public static void StartProcess()
        {
            computer.Open();

            List<IHardware> gpus = computer.Hardware
            .Where(h => h.HardwareType == HardwareType.GpuNvidia ||
                        h.HardwareType == HardwareType.GpuAmd ||
                        h.HardwareType == HardwareType.GpuIntel)
            .ToList();

            if (gpus.Count > 0)
            {
                hasGPU = true;
                gpu = gpus.First();
            }

            temperatureTimer = new System.Threading.Timer(GetTemperature, null, 0, updateInterval);
        }

        public static void GetTemperature(object state)
        {
            IHardware hardware = computer.Hardware.First(h => h.HardwareType == HardwareType.Cpu);
            
            hardware.Update();

            ISensor sensor = hardware.Sensors.First(s => s.Name.Equals("Core Average"));

            Console.WriteLine($"{sensor.Name}, {sensor.SensorType}, {sensor.Value}");

            if (sensor.Value != null)
            {
                Icon icon = ToIcon((float)sensor.Value);
                notifyIcon.Icon = icon;
                DestroyIcon(icon.Handle);
            }

            if (showGPU)
            {
                hardware = gpu;

                hardware.Update();

                sensor = hardware.Sensors.First(s => s.Name.Equals("GPU Core"));

                Console.WriteLine($"{sensor.Name}, {sensor.SensorType}, {sensor.Value}");

                if (sensor.Value != null)
                {
                    Icon icon = ToIconG((float)sensor.Value);
                    notifyIconG.Icon = icon;
                    DestroyIcon(icon.Handle);
                }
            } else
            {
                notifyIconG.Icon = null;
            }
        }

        public static Icon ToIcon(float value)
        {
            if (inFahrenheit)
                value = ToFahrenheit(value);

            return GetIconC((int)Math.Round((float)value));
        }

        public static Icon ToIconG(float value)
        {
            if (inFahrenheit)
                value = ToFahrenheit(value);

            return GetIconG((int)Math.Round((float)value));
        }

        public static float ToFahrenheit(float celcius)
        {
            return (celcius * 1.8f) + 32;
        }

        public static Icon GetIconC(int value)
        {
            int size = 32;

            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                string text = value.ToString();

                int fontSize = 28; // 32 max
                if (text.Length > 2)
                    fontSize = 20;

                using (Font font = new Font("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    // Measure text size
                    SizeF textSize = graphics.MeasureString(text, font);

                    // Compute centered position
                    float x = (size - textSize.Width) / 2;
                    float y = (size - textSize.Height) / 2;

                    // Draw centered text
                    graphics.DrawString(text, font, Brushes.White, x, y);
                }
            }

            IntPtr Hicon = bitmap.GetHicon();

            Icon newIcon = Icon.FromHandle(Hicon);

            return newIcon;
        }

        public static Icon GetIconG(int value)
        {
            int size = 32;

            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.DarkOliveGreen);

                string text = value.ToString();

                int fontSize = 26; // 32 max
                if (text.Length > 2)
                    fontSize = 20;

                using (Font font = new Font("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    // Measure text size
                    SizeF textSize = graphics.MeasureString(text, font);

                    // Compute centered position
                    float x = (size - textSize.Width) / 2;
                    float y = (size - textSize.Height) / 2;

                    // Draw centered text
                    graphics.DrawString(text, font, Brushes.White, x, y);
                }
            }

            IntPtr Hicon = bitmap.GetHicon();

            Icon newIcon = Icon.FromHandle(Hicon);

            return newIcon;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        public static ContextMenuStrip GetMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            // ===== Show GPU Checkbox =====
            ToolStripMenuItem showGpuItem = new ToolStripMenuItem("Show GPU")
            {
                Enabled = hasGPU,
                CheckOnClick = true,
                Checked = showGPU // initial state
            };

            showGpuItem.Click += (s, e) =>
            {
                showGPU = showGpuItem.Checked;
                Console.WriteLine($"Show GPU: {showGPU}");
                // You could also update your notifyIcon(s) visibility here
                // gpuNotifyIcon.Visible = showGPU;
            };

            menu.Items.Add(showGpuItem);

            // ===== Temperature Unit Submenu =====
            ToolStripMenuItem tempMenu = new ToolStripMenuItem("Temperature Unit");

            ToolStripMenuItem celsiusItem = new ToolStripMenuItem("Celsius") { CheckOnClick = true };
            ToolStripMenuItem fahrenheitItem = new ToolStripMenuItem("Fahrenheit") { CheckOnClick = true };

            celsiusItem.Click += (s, e) =>
            {
                celsiusItem.Checked = true;
                fahrenheitItem.Checked = false;
                inFahrenheit = false;
                Console.WriteLine("Temperature Unit: Celsius");
            };

            fahrenheitItem.Click += (s, e) =>
            {
                celsiusItem.Checked = false;
                fahrenheitItem.Checked = true;
                inFahrenheit = true;
                Console.WriteLine("Temperature Unit: Fahrenheit");
            };

            celsiusItem.Checked = true; // default
            tempMenu.DropDownItems.Add(celsiusItem);
            tempMenu.DropDownItems.Add(fahrenheitItem);
            menu.Items.Add(tempMenu);

            // ===== Update Interval Submenu =====
            ToolStripMenuItem intervalMenu = new ToolStripMenuItem("Update Interval");

            int[] intervals = { 250, 1000, 3000, 5000, 10000 }; // ms
            foreach (var ms in intervals)
            {
                string label = ms < 1000 ? $"{ms} ms" : $"{ms / 1000} s";
                ToolStripMenuItem item = new ToolStripMenuItem(label) { CheckOnClick = true };

                item.Click += (s, e) =>
                {
                    foreach (ToolStripMenuItem sibling in intervalMenu.DropDownItems)
                        sibling.Checked = false;

                    item.Checked = true;

                    temperatureTimer.Change(0, ms);
                    Console.WriteLine($"Update Interval: {label}");
                };

                intervalMenu.DropDownItems.Add(item);
            }

            ((ToolStripMenuItem)intervalMenu.DropDownItems[1]).Checked = true; // default 1s
            menu.Items.Add(intervalMenu);

            // ===== Exit =====
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) =>
            {
                notifyIcon.Visible = false;
                Application.Exit();
            };
            menu.Items.Add(exitItem);

            return menu;
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
