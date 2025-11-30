using VideoGenerator.Enums;

namespace VideoGenerator.Entities;

// All parts per all accounts
public class PublishBatch
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string GenerationQueueItemId { get; set; }
    public GenerationQueueItem GenerationQueueItem { get; set; }
    
    public ApproveStatus ApproveStatus { get; set; } = ApproveStatus.Waiting;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // To track whether batch is approved but not processed by worker to assign schedules and accounts
    public bool IsScheduled { get; set; }
    
    public List<PublishQueueItem> PublishQueueItems { get; set; } = [];
}