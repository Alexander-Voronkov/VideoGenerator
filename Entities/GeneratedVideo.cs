namespace VideoGenerator.Entities;

public class GeneratedVideo
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string BlobPath { get; set; }

	public string GenerationQueueId { get; set; }
	public GenerationQueueItem GenerationQueueItem { get; set; }
}
