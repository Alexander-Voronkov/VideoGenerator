using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using TikTokSplitter.Exceptions;
using TikTokSplitter.Models;
using TikTokSplitter.Services.Interfaces;

namespace TikTokSplitter.Services.Implementations;

public class TiktokMetadataScraperService : ITiktokMetadataScraperService
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;

    public TiktokMetadataScraperService(HttpClient client, ILogger<TiktokMetadataScraperService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<TiktokMetadataResponseDto> ScrapeData(string videoUrl)
    {
        try
        {
            using (var response = await _client.PostAsJsonAsync("", new StringContent(@$"{{""video_url"": ""{videoUrl}""}}")))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadFromJsonAsync<TiktokMetadataResponseDto>();
                return body;
            }
        }
        catch (Exception ex)
        {
            throw new TiktokDataRetrievingError($"Error occured while retrieving tiktok video metadata : {ex.Message}");
        }
    }
}
