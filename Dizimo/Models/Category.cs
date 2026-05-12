using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Dizimo.Models;

public class Category
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

    public override string ToString()
    {
        return $"{Title}";
    }
}