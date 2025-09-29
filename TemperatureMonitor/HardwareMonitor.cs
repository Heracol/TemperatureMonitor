using LibreHardwareMonitor.Hardware;

namespace TemperatureMonitor
{
    public class HardwareMonitor
    {
        private readonly Computer computer;

        private IHardware cpu;
        private ISensor? cpuSensor;

        private IHardware? gpu;
        private ISensor? gpuSensor;

        public bool HasGpu => gpu != null;

        public HardwareMonitor()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };

            computer.Open();

            cpu = computer.Hardware.First(h => h.HardwareType == HardwareType.Cpu);
            cpuSensor = cpu.Sensors.FirstOrDefault(s => s.Name.Equals("Core Average"));

            gpu = computer.Hardware
                .FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia ||
                                h.HardwareType == HardwareType.GpuAmd ||
                                h.HardwareType == HardwareType.GpuIntel);

            if (gpu != null)
            {
                gpuSensor = gpu.Sensors.FirstOrDefault(s => s.Name.Equals("GPU Core"));
            }
        }

        public float? GetCpuTemperature()
        {
            cpu.Update();
            return cpuSensor?.Value;
        }

        public float? GetGpuTemperature()
        {
            if (gpu == null)
                return null;

            gpu.Update();
            return gpuSensor?.Value;
        }

        public void Close() => computer.Close();
    }
}
