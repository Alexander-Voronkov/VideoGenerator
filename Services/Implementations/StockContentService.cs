using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class StockContentService : IStockContentService
{
	private readonly IPexelsService _pexelsService;

	public StockContentService(IPexelsService pexelsService)
	{
		_pexelsService = pexelsService;
	}

	public async Task<string> GetRandomVideoUrlAsync(string query)
	{
		return await _pexelsService.GetRandomVideoUrlAsync(query);
	}

	public async Task<string> GetRandomImageUrlAsync(string query)
	{
		return await _pexelsService.GetRandomImageUrlAsync(query);
	}
}
