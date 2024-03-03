namespace VideoGenerator.Exceptions;

public class InstallationFailedError : Exception
{
    public InstallationFailedError(string message) : base(message) { }
}
