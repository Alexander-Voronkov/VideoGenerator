namespace VideoGenerator.Exceptions;

public class VideoProcessingError : Exception
{
    public VideoProcessingError(string message) : base(message) { }
}
