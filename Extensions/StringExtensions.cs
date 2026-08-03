namespace VideoGenerator.Extensions;

public static class StringExtensions
{
    public static int ToFilmQuality(this string str)
    {
        return str switch
        {
            "1080p" => 1080,
            "720p" => 720,
            "480p" => 480,
            "360p" => 360,
            "SD" => 1,
            _ => 0,
        };
    }
}