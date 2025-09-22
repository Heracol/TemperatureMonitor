using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace TemperatureMonitor
{
    internal class Program
    {
        static Computer computer = new Computer
        {
            IsCpuEnabled = true
        };

        static void Main(string[] args)
        {
            StartProcess();

            

            Console.ReadLine();

            computer.Close();
        }

        public static void StartProcess()
        {
            computer.Open();

            new Timer(GetTemperature, null, 0, 1000);
        }

        public static void GetTemperature(object state)
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();

                ISensor sensor = hardware.Sensors.First(s => s.Name.Equals("Core Average"));

                Console.WriteLine($"{sensor.Name}, {sensor.SensorType}, {sensor.Value}");
            }
        }
    }
}
