namespace VideoGenerator.Infrastructure.Entities;

public class Group
{
    public long GroupId { get; set; }

    public string GroupLink { get; set; }

    public string GroupName { get; set; }

    public int TopicId { get; set; }

    public int LanguageId { get; set; }

    public bool IsTarget { get; set; }

    /// <summary>
    /// A topic related to this group
    /// </summary>
    public virtual Topic Topic { get; set; }

    /// <summary>
    /// A language related to this group
    /// </summary>
    public virtual Language Language { get; set; }

    /// <summary>
    /// Messages that will be published to this group
    /// </summary>
    public virtual ICollection<QueueMessage> InputQueueMessages { get; set; }

    /// <summary>
    /// Messages that are stolen from this group
    /// </summary>
    public virtual ICollection<QueueMessage> OutputQueueMessages { get; set; }

    /// <summary>
    /// Messages that were stolen from this group and published
    /// </summary>
    public virtual ICollection<PublishedMessage> OutputPublishedMessages { get; set; }

    /// <summary>
    /// Messages that were stolen and published to this group
    /// </summary>
    public virtual ICollection<PublishedMessage> InputPublishedMessages { get; set; }
}