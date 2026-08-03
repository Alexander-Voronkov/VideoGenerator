using Xabe.FFmpeg;

namespace VideoGenerator.Extensions;

public static partial class Extensions
{
    public static string ToFFMpegPosition(this Position pos, int offsetX, int offsetY)
    {
        var ffmpegPosition = pos switch
        {
            Position.UpperLeft => "x=0+{0}:y=0+{1}",
            Position.Up => "x=((w-text_w)/2)+{0}:y=0+{1}",
            Position.UpperRight => "x=(w-text_w)+{0}:y=0+{1}",
            Position.Center => "x=((w - text_w)/2)+{0}:y=((h-text_h)/2)+{1}",
            Position.BottomLeft => "x=0+{0}:y=(h-text_h)+{1}",
            Position.Bottom => "x=((w-text_w)/2)+{0}:y=(h-text_h)+{1}",
            Position.BottomRight => "x=(w-text_w)+{0}:y=(h-text_h)+{1}",
            Position.Left => "x=0+{0}:y=((h-text_h)/2)+{1}",
            Position.Right => "x=(w-text_w)+{0}:y=((h-text_h)/2)+{1}",
        };

        return string.Format(ffmpegPosition, offsetX, offsetY);
    }
}