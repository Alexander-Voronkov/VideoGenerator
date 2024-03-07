namespace VideoGenerator.Infrastructure.Entities;

public class Language
{
    public long LanguageId { get; set; }

    /// <summary>
    /// Language code in ISO 639-1
    /// </summary>
    public string LanguageCode { get; set; }

    public ICollection<Group> Groups { get; set; }
}