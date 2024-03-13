using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using VideoGenerator.Configurations;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class ChatService : IChatService
{
    private readonly OpenAIAPI _api;
    private readonly ILogger _logger;
    private readonly IOptions<Configuration> _configuration;

    public ChatService(IOptions<Configuration> config, ILogger<ChatService> logger, OpenAIAPI api)
    {
        _api = api;
        _logger = logger;
        _configuration = config;
    }

    public async Task<string> SendRequest(string topic, string languageCode)
    {
        ChatRequest chatRequest = new()
        {
            Temperature = 0.0,
            MaxTokens = 500,
            ResponseFormat = ChatRequest.ResponseFormats.JsonObject,
            Messages = new ChatMessage[]
            {
                new (ChatMessageRole.System, "You are a helpful assistant designed to output JSON."),
                new (ChatMessageRole.User, $"List 10 interesting string requests on the topic {topic} on the internet that would give me interesting videos and articles in the language: {languageCode}. Return JSON of a 'elements' array with the string values of topics."),
            }
        };

        var results = await _api.Chat.CreateChatCompletionAsync(chatRequest);
        var resultsAggregated = results.Choices.Aggregate("", (acc, elem) => string.Concat(elem.Message, ',', acc));

        _logger.LogInformation($"ChatGPT responded with: {resultsAggregated}");

        return resultsAggregated;
    }
}