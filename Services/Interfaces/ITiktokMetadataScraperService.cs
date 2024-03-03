using TikTokSplitter.Models;

namespace TikTokSplitter.Services.Interfaces;

public interface ITiktokMetadataScraperService
{
    Task<TiktokMetadataResponseDto> ScrapeData(string videoUrl);
}
