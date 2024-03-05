using System.Drawing;

namespace VideoGenerator.Extensions;

public static partial class Extensions
{
    public static string ToHexColor(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}