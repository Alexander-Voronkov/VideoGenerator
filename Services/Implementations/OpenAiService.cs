using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VideoGenerator.Configs;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class OpenAIService : IOpenAiService
{
	private readonly HttpClient _httpClient;
	private readonly string _apiKey;

	public OpenAIService(IOptions<OpenAiConfig> config)
	{
		_apiKey = config.Value.ApiKey;
		_httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://api.openai.com/v1/")
		};
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
	}

	public async Task<string> GetResponseAsync(string prompt)
	{
		var requestBody = new
		{
			model = "gpt-3.5-turbo",
			messages = new[]
			{
				new { role = "user", content = prompt }
			},
			max_tokens = 200
		};

		var content = new StringContent(
			JsonSerializer.Serialize(requestBody),
			Encoding.UTF8,
			"application/json"
		);

		var response = await _httpClient.PostAsync("chat/completions", content);
		response.EnsureSuccessStatusCode();

		var responseJson = await response.Content.ReadAsStringAsync();
		using var doc = JsonDocument.Parse(responseJson);
		var result = doc.RootElement
			.GetProperty("choices")[0]
			.GetProperty("message")
			.GetProperty("content")
			.GetString();

		return result?.Trim();
	}
}
