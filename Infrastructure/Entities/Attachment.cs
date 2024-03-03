using VideoGenerator.Enums;

namespace VideoGenerator.Infrastructure.Entities;
public class Attachment
{
    public int AttachmentId { get; set; }

    public long TelegramMessageId { get; set; }

    public string MimeType { get; set; }

    public byte[] Content { get; set; }

    public AttachmentContentType Type { get; set; }
}
