using System.Runtime.InteropServices;

namespace TemperatureMonitor
{
    public static class IconGenerator
    {
        public static Icon CreateIcon(int value, Color background, Color foreground, FontSize fontSize, bool addDegreeSymbol)
        {
            int size = 32;
            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(background);

                string text = value.ToString();

                int fontSizeValue = CalculateFontSize(text, fontSize, addDegreeSymbol);

                if (addDegreeSymbol)
                    text += "°";

                using Font font = new Font("Segoe UI", fontSizeValue, FontStyle.Regular, GraphicsUnit.Pixel);

                SizeF textSize = graphics.MeasureString(text, font);

                float x = (size - textSize.Width) / 2;
                float y = (size - textSize.Height) / 2;

                Brush brush = new SolidBrush(foreground);

                graphics.DrawString(text, font, brush, x, y);
            }

            IntPtr hIcon = bitmap.GetHicon();
            return Icon.FromHandle(hIcon);
        }

        private static int CalculateFontSize(string text, FontSize fontSize, bool addDegreeSymbol)
        {
            int fontSizeValue;

            if (text.Length > 2)
            {
                if (addDegreeSymbol)
                    fontSizeValue = 16; // fff°
                else fontSizeValue = 20; // fff
            }
            else
            {
                if (addDegreeSymbol)
                    fontSizeValue = 22; // cc°
                else fontSizeValue = 32; // cc
            }

            if (fontSize == FontSize.Medium)
                fontSizeValue -= 4;
            if (fontSize == FontSize.Small)
                fontSizeValue -= 8;

            return fontSizeValue;
        }

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr handle);
    }
}
