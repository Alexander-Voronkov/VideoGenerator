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
		var images = await _client.SearchPhotosAsync(query, orientation: "portrait", page: 1, pageSize: 1);
		var image = images.photos.Single();

		var found = await _client.GetPhotoAsync(image.id);
		return found.source.original;
	}

	public async Task<string> GetRandomVideoUrlAsync(string query)
	{
		var videos = await _client.SearchVideosAsync(query, orientation: "portrait", page: 1, pageSize: 1);
		var video = videos.videos.Single();

		var found = await _client.GetVideoAsync(video.id);
		return found.videoFiles.OrderByDescending(x => x.fps).ThenByDescending(x => x.quality).First().link;
	}
}
