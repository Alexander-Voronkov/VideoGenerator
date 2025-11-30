using VideoGenerator.Enums;

namespace VideoGenerator.Entities;

public class PublishAccount
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public UploadType Type { get; set; }
    
    public DateTime LastPublishAt { get; set; } = DateTime.UtcNow;
    
    public List<PublishQueueItem> PublishQueueItems { get; set; } = new();
}