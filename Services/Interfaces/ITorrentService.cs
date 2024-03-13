using VideoGenerator.Enums;

namespace VideoGenerator.Services.Interfaces;

public interface ITorrentService
{
    Task DownloadTorrent(string torrentFilePath, string outputPath = null, DataType type = DataType.Film, CancellationToken token = default);
}