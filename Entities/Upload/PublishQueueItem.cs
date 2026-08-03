namespace VideoGenerator.Entities;

// All parts per account
public class PublishQueueItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string PublishBatchId { get; set; }
    public PublishBatch PublishBatch { get; set; }
    
    public string PublishAccountId { get; set; }
    public PublishAccount PublishAccount { get; set; }
    
    public string PlaylistId { get; set; }
    
    public List<ScheduledUpload> ScheduledUploads { get; set; } = new();
}