using Microsoft.Extensions.Logging;
using VideoGenerator.Enums;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class TorrentService : ITorrentService
{
    private readonly ILogger _logger;

    public Task DownloadTorrent(string torrentFilePath, string outputPath = null, DataType type = DataType.Film, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}