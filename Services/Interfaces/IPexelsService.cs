namespace VideoGenerator.Services.Interfaces;

public interface IPexelsService
{
	Task<string> GetRandomVideoUrlAsync(string query);
	Task<string> GetRandomImageUrlAsync(string query);
}
