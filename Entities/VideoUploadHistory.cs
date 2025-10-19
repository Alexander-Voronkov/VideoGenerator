namespace VideoGenerator.Entities;

public class VideoUploadHistory
{
	public string Id { get; set; } = Guid.NewGuid().ToString();

	public string GeneratedVideoId { get; set; }
	public GeneratedVideo GeneratedVideo { get; set; }

	public string Metadata { get; set; }

	public UploadType UploadType { get; set; }

	public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
