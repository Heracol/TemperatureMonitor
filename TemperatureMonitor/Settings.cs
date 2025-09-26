using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TemperatureMonitor
{
    public class Settings
    {
        private const string filePath = "settings.json";

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

        public void Save()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                options.Converters.Add(new ColorJsonConverter());

                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            } 
            catch { }
        }

        public void Load()
        {
            if (!File.Exists(filePath)) 
                return;

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                options.Converters.Add(new ColorJsonConverter());

                string json = File.ReadAllText(filePath);
                Settings loaded = JsonSerializer.Deserialize<Settings>(json, options);
                
                if (loaded != null)
                {
                    showGpu = loaded.ShowGpu;
                    inFahrenheit = loaded.InFahrenheit;
                    updateInterval = loaded.UpdateInterval;
                    fontSize = loaded.fontSize;
                    showDegreeSymbol = loaded.showDegreeSymbol;
                }
            }
            catch { }
        }
    }
}
