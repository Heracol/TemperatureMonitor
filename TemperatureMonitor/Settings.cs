using System.Text.Json;
using System.Text.Json.Serialization;

namespace TemperatureMonitor
{
    public class Settings
    {
        private static readonly string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TemperatureMonitor");

        private static readonly string filePath = Path.Combine(folderPath, "settings.json");

        private bool showGpu = false;
        public bool ShowGpu
        {
            get => showGpu;
            set
            {
                showGpu = value;
                Save();
            }
        }

        private bool inFahrenheit = false;
        public bool InFahrenheit
        {
            get => inFahrenheit;
            set
            {
                inFahrenheit = value;
                Save();
            }
        }

        private int updateInterval = 1000;
        public int UpdateInterval
        {
            get => updateInterval;
            set
            {
                updateInterval = value;
                Save();
            }
        }

        private FontSize fontSize = FontSize.Medium;
        [JsonPropertyName("FontSize")]
        public FontSize FontSizeValue
        {
            get => fontSize;
            set
            {
                fontSize = value;
                Save();
            }
        }

        private bool showDegreeSymbol = false;
        public bool ShowDegreeSymbol
        {
            get => showDegreeSymbol;
            set
            {
                showDegreeSymbol = value;
                Save();
            }
        }

        private Color cpuBackgroundColor = Color.Transparent;
        public Color CpuBackgroundColor
        {
            get => cpuBackgroundColor;
            set
            {
                cpuBackgroundColor = value;
                Save();
            }
        }

        private Color cpuTextColor = Color.White;
        public Color CpuTextColor
        {
            get => cpuTextColor;
            set
            {
                cpuTextColor = value;
                Save();
            }
        }

        private Color gpuBackgroundColor = Color.Transparent;
        public Color GpuBackgroundColor
        {
            get => gpuBackgroundColor;
            set
            {
                gpuBackgroundColor = value;
                Save();
            }
        }

        private Color gpuTextColor = Color.LightGreen;
        public Color GpuTextColor
        {
            get => gpuTextColor;
            set
            {
                gpuTextColor = value;
                Save();
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(folderPath);

                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                options.Converters.Add(new ColorJsonConverter());

                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public static Settings Load()
        {
            if (!File.Exists(filePath))
            {
                Settings settings = new Settings();
                settings.Save();

                return settings;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new ColorJsonConverter() }
                };

                string json = File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<Settings>(json, options) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }
    }
}
