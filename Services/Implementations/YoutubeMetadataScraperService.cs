using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using TikTokSplitter.Exceptions;
using TikTokSplitter.Extensions;
using TikTokSplitter.Models;
using TikTokSplitter.Services.Interfaces;

namespace TikTokSplitter.Services.Implementations;

public class YoutubeMetadataScraperService : IYoutubeMetadataScraperService
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public YoutubeMetadataScraperService(HttpClient client, ILogger<YoutubeMetadataScraperService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<YoutubeMetadataResponseDto> ScrapeData(string videoUrl)
    {
        try
        {
            videoUrl = videoUrl.GetYouTubeVideoId() ?? throw new Exception("Video url is wrong.");

            using (var response = await _client.GetAsync($"https://ytstream-download-youtube-videos.p.rapidapi.com/dl?id={videoUrl}"))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadFromJsonAsync<YoutubeMetadataResponseDto>();
                body.Title = body.Title.EscapeForbiddenCharacters();

                _logger.LogInformation("Youtube video metadata has been successfully retrieved...");
                return body;
            }
        }
        catch (Exception ex)
        {
            throw new YoutubeDataRetrievingError($"Error while scraping metadata: {ex.Message}");
        }
    }
}