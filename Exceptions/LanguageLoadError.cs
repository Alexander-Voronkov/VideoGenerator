namespace VideoGenerator.Exceptions;

public class LanguageLoadError : Exception
{
    public LanguageLoadError(string message) : base(message) { }
}
