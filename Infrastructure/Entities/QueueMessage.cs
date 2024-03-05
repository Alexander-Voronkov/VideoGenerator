namespace VideoGenerator.Infrastructure.Entities;

public class QueueMessage
{
    public long QueueMessageId { get; set; }

    public long SourceGroupId { get; set; }

    public long TargetGroupId { get; set; }

    public string Text { get; set; }

    public DateTime StolenAt { get; set; }

    /// <summary>
    /// A group this message will be published to
    /// </summary>
    public virtual Group TargetGroup { get; set; }

    /// <summary>
    /// A group this message is stolen from
    /// </summary>
    public virtual Group SourceGroup { get; set; }

    /// <summary>
    /// This message`s attachments
    /// </summary>
    public virtual ICollection<Attachment> Attachments { get; set; }
}