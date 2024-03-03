namespace VideoGenerator.Infrastructure.Entities;

public class Group
{
    public string GroupId { get; set; }
    public string TopicId { get; set; }
    public virtual Topic Topic { get; set; }
    public virtual IEnumerable<Message> StolenMessages { get; set; }
}