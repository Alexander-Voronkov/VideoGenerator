namespace VideoGenerator.Infrastructure.Entities;

public class Language
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; }
    public ICollection<Topic> Topics { get; set; }
}