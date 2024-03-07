using VideoGenerator.Enums;

namespace VideoGenerator.Infrastructure.Entities;

public class Attachment
{
    public long AttachmentId { get; set; }

    public long QueueMessageId { get; set; }

    public string MimeType { get; set; }

    public byte[] Content { get; set; }

    public AttachmentContentType Type { get; set; }

    /// <summary>
    /// Message this file refers to
    /// </summary>
    public virtual QueueMessage QueueMessage { get; set; }

    /// <summary>
    /// Published message this file refers to
    /// </summary>
    public virtual ICollection<PublishedMessage> PublishedMessages { get; set; }
}