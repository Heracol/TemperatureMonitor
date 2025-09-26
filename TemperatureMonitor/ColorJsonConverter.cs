using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TemperatureMonitor
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            var parts = value.Split(',');
            if (parts.Length == 4 &&
                byte.TryParse(parts[0], out byte a) &&
                byte.TryParse(parts[1], out byte r) &&
                byte.TryParse(parts[2], out byte g) &&
                byte.TryParse(parts[3], out byte b))
            {
                return Color.FromArgb(a, r, g, b);
            }
            return Color.Transparent;
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            string rgba = $"{value.A},{value.R},{value.G},{value.B}";
            writer.WriteStringValue(rgba);
        }
    }
}
