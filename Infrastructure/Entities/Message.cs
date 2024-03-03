namespace VideoGenerator.Infrastructure.Entities;

public class Message
{
    public string MessageId { get; set; }
    public string GroupId { get; set; }
    public virtual Group Group { get; set; }
}