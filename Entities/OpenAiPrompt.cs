namespace VideoGenerator.Entities;

public class OpenAiPrompt
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string Title { get; set; }
	public string Prompt { get; set; }
	public DateTime AddedAt { get; set; }
}
