using System.Collections.Concurrent;
using TikTokSplitter.Enums;

namespace VideoGenerator.Constants;

public static class SourceContentTypeConstants
{
    private static readonly ConcurrentQueue<SourceContentType> _contentTypes;

    static SourceContentTypeConstants()
    {
        _contentTypes = new(
        [
            SourceContentType.Youtube,
            SourceContentType.Tiktok,
            SourceContentType.Habr,
            SourceContentType.TelegramChannel,
            SourceContentType.Reels,
        ]);
    }

    public static Task<SourceContentType> GetNext()
    {
        _contentTypes.TryDequeue(out var contentType);
        _contentTypes.Enqueue(contentType);

        return Task.FromResult(contentType);
    }
}