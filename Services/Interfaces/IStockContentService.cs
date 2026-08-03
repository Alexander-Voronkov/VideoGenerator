namespace VideoGenerator.Services.Interfaces;

public interface IStockContentService
{
	Task<string> GetRandomVideoUrlAsync(string query);
	Task<string> GetRandomImageUrlAsync(string query);
}
