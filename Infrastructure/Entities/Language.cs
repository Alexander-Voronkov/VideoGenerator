namespace VideoGenerator.Infrastructure.Entities;

public class Language
{
    public int LanguageId { get; set; }

    /// <summary>
    /// Language code in ISO 639-1
    /// </summary>
    public string LanguageCode { get; set; }

    public virtual ICollection<Topic> Topics { get; set; }
}