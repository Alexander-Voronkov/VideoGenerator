using VideoGenerator.Enums;

namespace VideoGenerator.Entities;

public class GeneratedVideo
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string BlobPath { get; set; }
	public UploadingStatus UploadingStatus { get; set; }

	public VideoType Type { get; set; }

	public string GenerationQueueId { get; set; }
	public GenerationQueueItem GenerationQueueItem { get; set; }
	
	public int PartNumber { get; set; }
	public int TotalParts { get; set; }
}
