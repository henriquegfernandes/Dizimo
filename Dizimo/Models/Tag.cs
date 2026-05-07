using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Dizimo.Models
{
    public class Tag
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Color { get; set; } = "#FF0000";

        [JsonIgnore]
        public IBrush ColorBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush(ColorHelper.ParseColor(Color));
                }
                catch
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }
        }

        [JsonIgnore]
        public Avalonia.Media.Color DisplayColor
        {
            get
            {
                return ColorHelper.ParseColor(Color);
            }
        }

        [JsonIgnore]
        public Avalonia.Media.Color DisplayDarkColor
        {
            get
            {
                var color = DisplayColor;
                // Escurecer cor em 20%
                return new Avalonia.Media.Color(
                    color.A,
                    (byte)(color.R * 0.8),
                    (byte)(color.G * 0.8),
                    (byte)(color.B * 0.8)
                );
            }
        }

        [JsonIgnore]
        public Avalonia.Media.Color DisplayLightColor
        {
            get
            {
                var color = DisplayColor;
                // Clarear cor em 20%
                var r = (int)(color.R + (255 - color.R) * 0.2);
                var g = (int)(color.G + (255 - color.G) * 0.2);
                var b = (int)(color.B + (255 - color.B) * 0.2);
                return new Avalonia.Media.Color(color.A, (byte)r, (byte)g, (byte)b);
            }
        }

        [JsonIgnore]
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Helper para conversão de cores hex para Avalonia Color
    /// </summary>
    public static class ColorHelper
    {
        public static Avalonia.Media.Color ParseColor(string colorString)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                return Colors.Red;

            try
            {
                // Remove '#' se existir
                var hex = colorString.StartsWith('#') ? colorString.Substring(1) : colorString;

                if (hex.Length == 6)
                {
                    var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    return new Avalonia.Media.Color(255, r, g, b);
                }
                else if (hex.Length == 8)
                {
                    var a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var r = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    return new Avalonia.Media.Color(a, r, g, b);
                }
            }
            catch { }

            return Colors.Red;
        }
    }
}

