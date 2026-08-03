namespace VideoGenerator.Entities;

public class ScheduledUpload
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string PublishQueueItemId { get; set; }
    public PublishQueueItem PublishQueueItem { get; set; }
    
    public string GeneratedVideoId { get; set; }
    public GeneratedVideo GeneratedVideo { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    
    public UploadingStatus Status { get; set; } = UploadingStatus.NotUploaded;
}