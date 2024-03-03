using Xabe.FFmpeg;

namespace TikTokSplitter.Constants;

public class FFmpegPositionConstants
{
    private static readonly Dictionary<Position, string> _positions;

    static FFmpegPositionConstants()
    {
        _positions =
            new()
            {
                [Position.UpperLeft] = "x=0{0}:y=0{1}",
                [Position.Up] = "x=((w-text_w)/2){0}:y=0{1}",
                [Position.UpperRight] = "x=(w-text_w){0}:y=0{1}",
                [Position.Center] = "x=((w - text_w)/2){0}:y=((h-text_h)/2){1}",
                [Position.BottomLeft] = "x=0{0}:y=(h-text_h){1}",
                [Position.Bottom] = "x=((w-text_w)/2){0}:y=(h-text_h){1}",
                [Position.Bottom] = "x=(w-text_w){0}:y=(h-text_h){1}",
            };
    }

    public static IReadOnlyDictionary<Position, string> Positions => _positions;
}