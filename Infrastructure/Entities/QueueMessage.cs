namespace VideoGenerator.Infrastructure.Entities;

public class QueueMessage
{
    public long QueueMessageId { get; set; }

    public long SourceGroupId { get; set; }

    public long PublishedMessageId { get; set; }

    public string Text { get; set; }

    public DateTime StolenAt { get; set; }

    /// <summary>
    /// A group this message is stolen from
    /// </summary>
    public virtual Group SourceGroup { get; set; }

    /// <summary>
    /// This message`s attachments
    /// </summary>
    public virtual ICollection<Attachment> Attachments { get; set; }
}