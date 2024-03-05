using System.Collections.Concurrent;
using VideoGenerator.Enums;

namespace VideoGenerator.AllConstants;

public static partial class Constants
{
    private static readonly ConcurrentQueue<SourceContentType> _contentTypes =
        new([
            SourceContentType.Youtube,
            SourceContentType.Tiktok,
            SourceContentType.Habr,
            SourceContentType.TelegramChannel,
            SourceContentType.Reels,
        ]);

    public static Task<SourceContentType> GetNext()
    {
        _contentTypes.TryDequeue(out var contentType);
        _contentTypes.Enqueue(contentType);

        return Task.FromResult(contentType);
    }
}