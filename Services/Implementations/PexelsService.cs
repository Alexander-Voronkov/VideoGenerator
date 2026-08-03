using Microsoft.Extensions.Options;
using VideoGenerator.Configs;
using VideoGenerator.Helpers;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class PexelsService : IPexelsService
{
	private readonly PexelsClient _client;
	private readonly PexelsStockContentConfig _config;

	public PexelsService(IOptions<PexelsStockContentConfig> config)
	{
		_config = config.Value;
		_client = new(_config.ApiKey);
	}

	public async Task<string> GetRandomImageUrlAsync(string query)
	{
		var images = await _client.SearchPhotosAsync(query, orientation: "portrait", page: 1, pageSize: 10);
		var found = images.photos.ElementAtOrDefault(Random.Shared.Next(0, 9));

		if (found is null)
		{
			return null;
		}

		return found.source.original;
	}

	public async Task<string> GetRandomVideoUrlAsync(string query)
	{
		var videos = await _client.SearchVideosAsync(query, orientation: "portrait", page: 1, pageSize: 10);
		var found = videos.videos.ElementAtOrDefault(Random.Shared.Next(0, 9));

		if (found is null)
		{
			return null;
		}

		return found.videoFiles.OrderByDescending(x => x.fps).ThenByDescending(x => x.quality).First().link;
	}
}
