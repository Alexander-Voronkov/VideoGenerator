using TL;

namespace VideoGenerator.Extensions;

public static partial class Extensions
{
    public static string ToMime(this Storage_FileType type)
    {
        return type switch
        {
            Storage_FileType.jpeg
            or Storage_FileType.gif
            or Storage_FileType.png
            or Storage_FileType.webp => $"image/{type}",
            Storage_FileType.pdf => $"application/{type}",
            Storage_FileType.mp3 => $"audio/{type}",
            Storage_FileType.mp4 => $"video/{type}",
            Storage_FileType.mov => "video/quicktime",
            _ => string.Empty,
        };
    }
}
