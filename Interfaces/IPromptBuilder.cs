using OpenAI_API.Chat;
using SEO_Optimizer.Models;

namespace SEO_Optimizer.Interfaces;

public interface IPromptBuilder
{
    List<ChatMessage> BuildPrompts(KeywordType[] keywordTypes, string? contentPrompt);
}
