using TikTokSplitter.Models;

namespace TikTokSplitter.Services.Interfaces;

public interface IYoutubeMetadataScraperService
{
    Task<YoutubeMetadataResponseDto> ScrapeData(string videoUrl);
}
