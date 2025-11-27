namespace VideoGenerator.Entities;

public class InterestingFactQueueItem
{
	public string Id { get; set; }
	public GenerationStatus Status { get; set; }

	public string YoutubePlaylistId { get; set; }

	public string Title { get; set; }
	public string Text { get; set; }
	public string Description { get; set; }
	public List<string> Tags { get; set; }
}
