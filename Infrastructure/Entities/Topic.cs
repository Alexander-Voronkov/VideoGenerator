namespace VideoGenerator.Infrastructure.Entities;

public class Topic
{
    public long TopicId { get; set; }

    public string TopicName { get; set; }

    /// <summary>
    /// Groups on this topic
    /// </summary>
    public virtual ICollection<Group> Groups { get; set; }

    /// <summary>
    /// Languages this topic available for
    /// </summary>
    public virtual ICollection<Language> AvailableLanguages { get; set; }
}