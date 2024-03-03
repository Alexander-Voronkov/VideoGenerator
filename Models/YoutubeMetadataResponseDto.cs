namespace TikTokSplitter.Models;

public class YoutubeMetadataResponseDto
{
    public string Title { get; set; }
    public string Id { get; set; }
    public string Status { get; set; }
    public string ChannelTitle { get; set; }
    public string ChannelId { get; set; }
    public string Description { get; set; }
    public bool AllowRatings { get; set; }
    public string ViewCount { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsUnpluggedCorpus { get; set; }
    public bool IsLiveContent { get; set; }
    public bool ExpiresInSeconds { get; set; }
    public string PmReg { get; set; }
    public bool IsProxied { get; set; }
    public int LengthSeconds { get; set; }
    public IEnumerable<string> Keywords { get; set; }
    public IEnumerable<Format> Formats { get; set; }
    public IEnumerable<AdaptiveFormat> AdaptiveFormats { get; set; }
    public IEnumerable<Thumbnail> Thumbnails { get; set; }
}

public class Thumbnail
{
    public string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Format
{
    public string Url { get; set; }
    public string Itag { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Fps { get; set; }
    public string QualityLabel { get; set; }
    public string MimeType { get; set; }
    public int Bitrate { get; set; }
    public string LastModified { get; set; }
    public string Quality { get; set; }
    public string ProjectionType { get; set; }
    public string AudioQuality { get; set; }
    public string ApproxDurationMs { get; set; }
    public string AudioSampleRate { get; set; }
    public string AudioChannels { get; set; }
}

public class AdaptiveFormat
{
    public int Itag { get; set; }
    public string Url { get; set; }
    public string MimeType { get; set; }
    public int Bitrate { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Range InitRange { get; set; }
    public Range IndexRange { get; set; }
    public string LastModified { get; set; }
    public string ContentLength { get; set; }
    public string Quality { get; set; }
    public int Fps { get; set; }
    public string QualityLabel { get; set; }
    public string ProjectionType { get; set; }
    public int AverageBitrate { get; set; }
    public string ApproxDurationMs { get; set; }
    public bool HighReplication { get; set; }
    public string AudioQuality { get; set; }
    public string AudioSampleRate { get; set; }
    public float LoudnessDb { get; set; }
    public int AudioChannels { get; set; }
    public ColorInfo ColorInfo { get; set; }
}

public class Range
{
    public string Start { get; set; }
    public string End { get; set; }
}

public class ColorInfo
{
    public string Primaries { get; set; }
    public string TransferCharacteristics { get; set; }
    public string MatrixCoefficients { get; set; }
}
