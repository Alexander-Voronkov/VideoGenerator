namespace TikTokSplitter.Services.Interfaces;

public interface IVideoDownloaderService
{
    Task Download(string videoUrl, string outputPath);
}
