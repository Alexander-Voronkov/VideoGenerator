namespace VideoGenerator.Infrastructure.Entities;

public class Group
{
    public long GroupId { get; set; }
    public int? TopicId { get; set; }
    public virtual Topic Topic { get; set; }
    public virtual IEnumerable<TelegramMessage> StolenMessages { get; set; }
}