using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
            IsCpuEnabled = true
        };

        static NotifyIcon notifyIcon = new NotifyIcon();

        static void Main(string[] args)
        {
            SetProcessDPIAware();

            StartProcess();

            notifyIcon.ContextMenuStrip = GetMenu();

            notifyIcon.Visible = true;

            Application.ApplicationExit += new EventHandler(Close);

            Application.Run();
        }

        public static void Close(object sender, EventArgs e)
        {
            computer.Close();

            notifyIcon.Icon.Dispose();
            notifyIcon.Dispose();
        }

        public static void StartProcess()
        {
            computer.Open();

            new System.Threading.Timer(GetTemperature, null, 0, 1000);
        }

        public static void GetTemperature(object state)
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();

                ISensor sensor = hardware.Sensors.First(s => s.Name.Equals("Core Average"));

                Console.WriteLine($"{sensor.Name}, {sensor.SensorType}, {sensor.Value}");

                if (sensor.Value != null)
                {
                    Icon icon = GetIcon((int)Math.Round((float)sensor.Value));
                    notifyIcon.Icon = icon;
                    DestroyIcon(icon.Handle);
                }
            }
        }

        public static Icon GetIcon(int value)
        {
            int size = 32;

            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                string text = value.ToString();

                using (Font font = new Font("Roboto", 24, FontStyle.Bold, GraphicsUnit.Pixel))
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

            // ===== Temperature Unit Submenu =====
            ToolStripMenuItem tempMenu = new ToolStripMenuItem("Temperature Unit");

            ToolStripMenuItem celsiusItem = new ToolStripMenuItem("Celsius") { CheckOnClick = true };
            ToolStripMenuItem fahrenheitItem = new ToolStripMenuItem("Fahrenheit") { CheckOnClick = true };

            // Make them behave like radio buttons
            celsiusItem.Click += (s, e) =>
            {
                celsiusItem.Checked = true;
                fahrenheitItem.Checked = false;
                Console.WriteLine("Temperature Unit: Celsius");
            };

            fahrenheitItem.Click += (s, e) =>
            {
                celsiusItem.Checked = false;
                fahrenheitItem.Checked = true;
                Console.WriteLine("Temperature Unit: Fahrenheit");
            };

            // Default selection
            celsiusItem.Checked = true;

            tempMenu.DropDownItems.Add(celsiusItem);
            tempMenu.DropDownItems.Add(fahrenheitItem);
            menu.Items.Add(tempMenu);

            // ===== Update Interval Submenu =====
            ToolStripMenuItem intervalMenu = new ToolStripMenuItem("Update Interval");

            int[] intervals = { 250, 1000, 3000, 5000, 10000 }; // in ms
            foreach (var ms in intervals)
            {
                string label = ms < 1000 ? $"{ms} ms" : $"{ms / 1000} s";
                ToolStripMenuItem item = new ToolStripMenuItem(label) { CheckOnClick = true };

                item.Click += (s, e) =>
                {
                    // Uncheck all siblings
                    foreach (ToolStripMenuItem sibling in intervalMenu.DropDownItems)
                        sibling.Checked = false;

                    item.Checked = true;
                    Console.WriteLine($"Update Interval: {label}");
                };

                intervalMenu.DropDownItems.Add(item);
            }

            // Default selection: 1s
            ((ToolStripMenuItem)intervalMenu.DropDownItems[1]).Checked = true;

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
