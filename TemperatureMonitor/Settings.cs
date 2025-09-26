using System;
using System.Collections.Generic;
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

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
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
                string json = File.ReadAllText(filePath);
                Settings loaded = JsonSerializer.Deserialize<Settings>(json);
                
                if (loaded != null)
                {
                    showGpu = loaded.ShowGpu;
                    inFahrenheit = loaded.InFahrenheit;
                    updateInterval = loaded.UpdateInterval;
                }
            }
            catch { }
        }
    }
}
