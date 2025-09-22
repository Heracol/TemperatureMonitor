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
            StartProcess();

            notifyIcon.Visible = true;

            Application.Run();

            computer.Close();
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
            Bitmap bitmap = new Bitmap(32, 32);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                Font font = new Font("Roboto", 16, FontStyle.Bold);
                graphics.DrawString(value.ToString(), font, Brushes.White, new PointF(4, 4));
            }

            IntPtr Hicon = bitmap.GetHicon();

            Icon newIcon = Icon.FromHandle(Hicon);

            return newIcon;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);
    }
}
