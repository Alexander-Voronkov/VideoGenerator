namespace VideoGenerator.Exceptions;

public class YoutubeDataRetrievingError : Exception
{
    public YoutubeDataRetrievingError(string message) : base(message) { }
}
