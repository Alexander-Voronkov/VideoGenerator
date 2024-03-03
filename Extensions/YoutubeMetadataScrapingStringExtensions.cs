using System.Text.RegularExpressions;

namespace TikTokSplitter.Extensions;

public static class YoutubeMetadataScrapingStringExtensions
{
    public static string GetYouTubeVideoId(this string youtubeUrl)
    {
        string pattern = @"(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})";

        Regex regex = new Regex(pattern);

        Match match = regex.Match(youtubeUrl);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        else
        {
            return null;
        }
    }

    public static string EscapeForbiddenCharacters(this string input)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();

        foreach (char invalidChar in invalidChars)
        {
            input = input.Replace(invalidChar, '_');
        }

        input = input.Replace(' ', '_');

        return input;
    }
}
