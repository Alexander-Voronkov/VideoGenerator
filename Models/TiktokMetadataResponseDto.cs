namespace VideoGenerator.Models;

public class TiktokMetadataResponseDto
{
    public Author Author { get; set; }
    public string Desc { get; set; }
    public int Duration { get; set; }
    public Music Music { get; set; }
    public string ShareUrl { get; set; }
    public string Status { get; set; }
    public string UrlType { get; set; }
    public Video Video { get; set; }
    public List<object> VideoHashtags { get; set; }
}

public class AvatarMedium
{
    public int Height { get; set; }
    public string Uri { get; set; }
    public List<string> UrlList { get; set; }
    public object UrlPrefix { get; set; }
    public int Width { get; set; }
}

public class Avatar
{
    public AvatarMedium AvatarMedium { get; set; }
}

public class Author
{
    public Avatar Avatar { get; set; }
    public string Nickname { get; set; }
    public string Region { get; set; }
    public string Uid { get; set; }
    public string VideoAuthorDiggCount { get; set; }
    public string VideoAuthorFollowerCount { get; set; }
    public string VideoAuthorFollowingCount { get; set; }
    public string VideoAuthorHeartCount { get; set; }
    public string VideoAuthorVideoCount { get; set; }
}

public class CoverMedium
{
    public int Height { get; set; }
    public string Uri { get; set; }
    public List<string> UrlList { get; set; }
    public object UrlPrefix { get; set; }
    public int Width { get; set; }
}

public class Cover
{
    public string CoverMedium { get; set; }
}

public class Music
{
    public Cover Cover { get; set; }
    public List<string> DownloadUrl { get; set; }
    public int Duration { get; set; }
    public string OwnerNickname { get; set; }
    public string Title { get; set; }
}

public class Video
{
    public string Cover { get; set; }
    public WithWatermark WithWatermark { get; set; }
    public WithoutWatermark WithoutWatermark { get; set; }
}

public class WithWatermark
{
    public int DataSize { get; set; }
    public int Height { get; set; }
    public List<string> UrlList { get; set; }
    public int Width { get; set; }
}

public class WithoutWatermark
{
    public int DataSize { get; set; }
    public int Height { get; set; }
    public List<string> UrlList { get; set; }
    public int Width { get; set; }
}