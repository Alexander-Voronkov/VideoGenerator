using System.Drawing;

namespace TikTokSplitter.Extensions;

public static class ColorExtensions
{
    public static string ToHexColor(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}