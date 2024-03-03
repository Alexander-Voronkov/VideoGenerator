namespace VideoGenerator.Infrastructure.Entities;

public class Topic
{
    public int TopicId { get; set; }
    public string TopicName { get; set; }
    public int LanguageId { get; set; }
    public Language Language { get; set; }
}