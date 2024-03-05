namespace VideoGenerator.Infrastructure.Entities;

public class Topic
{
    public long TopicId { get; set; }

    public string TopicName { get; set; }

    public int LanguageId { get; set; }

    /// <summary>
    /// Groups on this topic
    /// </summary>
    public virtual ICollection<Group> Groups { get; set; }

    /// <summary>
    /// Language this topic refers to
    /// </summary>
    public virtual Language Language { get; set; }
}