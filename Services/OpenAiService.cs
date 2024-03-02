using System.Net;
using System.Runtime.ExceptionServices;
using OpenAI_API;
using OpenAI_API.Chat;
using SEO_Optimizer.Interfaces;
using SEO_Optimizer.Utils;

namespace SEO_Optimizer.Services;

public class OpenAiService : IOpenAiInterface
{
    private readonly OpenAIAPI _openAiClient;
    private readonly ILogger<OpenAiService> _logger;
    private readonly OpenAiSettings _settings;
    
    public OpenAiService(string? apiKey, ILogger<OpenAiService> logger, OpenAiSettings? settings)
    {
         _openAiClient = new OpenAIAPI(apiKey);
         _logger = logger;
         _settings = settings ?? new OpenAiSettings() { maxTokens = 500, temperature = 0.0 };
    }
    
    public Task<ChatResult> SendChatMessages(List<ChatMessage> messages)
    {
        return SendChat(BuildRequest(messages));
    }

    private ChatRequest BuildRequest(List<ChatMessage> messages)
    {
        return new ChatRequest()
        {
            Model = OpenAI_API.Models.Model.GPT4_Turbo,
            Temperature = _settings.temperature,
            MaxTokens = _settings.maxTokens,
            ResponseFormat = ChatRequest.ResponseFormats.JsonObject,
            Messages = messages
        };
    }
    
    private async Task<ChatResult> SendChat(ChatRequest chatRequest)
    {
        try
        {
            var results = await _openAiClient.Chat.CreateChatCompletionAsync(chatRequest);
            return results;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request to OpenAI failed.");
            throw new InvalidOperationException("A network error occurred while communicating with OpenAI.", httpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while communicating with OpenAI.");
            throw;
        }
    }
}