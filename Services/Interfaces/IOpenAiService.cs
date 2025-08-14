namespace VideoGenerator.Services.Interfaces;

public interface IOpenAiService
{
	Task<string> GetResponseAsync(string prompt);
}
