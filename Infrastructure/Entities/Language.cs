namespace VideoGenerator.Infrastructure.Entities;

public class Language
{
    public string LanguageId { get; set; }
    public string LanguageCode { get; set; }
    public string TopicId { get; set; }
    public virtual Topic Topic { get; set; }
}