namespace TikTokSplitter.Exceptions;

public class VideoProcessingError : Exception
{
    public VideoProcessingError(string message) : base(message) { }
}
