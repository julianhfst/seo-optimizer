using OpenAI_API.Chat;

namespace SEO_Optimizer.Interfaces;

public interface IOpenAiInterface
{
    public Task<ChatResult> SendChatMessages(List<ChatMessage> messages);
}