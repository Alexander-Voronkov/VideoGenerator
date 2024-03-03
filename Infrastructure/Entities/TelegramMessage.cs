namespace VideoGenerator.Infrastructure.Entities;

public class TelegramMessage
{
    public long TelegramMessageId { get; set; }
    public long GroupId { get; set; }
    public Group Group { get; set; }
    public string Text { get; set; }

    public ICollection<Attachment> Attachments { get; set; }
}