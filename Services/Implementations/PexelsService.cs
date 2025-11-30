using Microsoft.Extensions.Options;
using PexelsDotNetSDK.Api;
using VideoGenerator.Configs;
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
		var images = await _client.SearchPhotosAsync(query, orientation: "portrait", page: Random.Shared.Next(1, 10), pageSize: 10);
		var found = images.photos.First();

		return found.source.original;
	}

	public async Task<string> GetRandomVideoUrlAsync(string query)
	{
		var videos = await _client.SearchVideosAsync(query, orientation: "portrait", page: Random.Shared.Next(1, 10), pageSize: 10);
		var found = videos.videos.First();

		return found.videoFiles.OrderByDescending(x => x.fps).ThenByDescending(x => x.quality).First().link;
	}
}
