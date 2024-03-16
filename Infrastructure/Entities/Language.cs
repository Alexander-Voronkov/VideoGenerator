namespace VideoGenerator.Infrastructure.Entities;

public class Language
{
    public int LanguageId { get; set; }

    /// <summary>
    /// Language code in ISO 639-1 or 639-3
    /// </summary>
    public string LanguageCode { get; set; }

    public bool IsAvailable { get; set; }

    public virtual ICollection<Topic> Topics { get; set; }

    public virtual ICollection<Group> Groups { get; set; }
}