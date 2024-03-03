namespace TikTokSplitter.Exceptions;

public class SubtitleGenerationError : Exception
{
    public SubtitleGenerationError(string error) : base(error) { }
}
