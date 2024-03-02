using System.Runtime.ExceptionServices;
using System.Text.Json;
using OpenAI_API.Chat;
using SEO_Optimizer.Interfaces;
using SEO_Optimizer.Models;

namespace SEO_Optimizer.Utils;

public class PromptBuilder : IPromptBuilder
{
    private readonly string _fileName = "Resources/prompts.json";
    private readonly ILogger<PromptBuilder> _logger;

    public PromptBuilder(ILogger<PromptBuilder> logger)
    {
        _logger = logger;
    }

    public List<ChatMessage> BuildPrompts(KeywordType[] keywordTypes, string? contentPrompt)
    {
        if (!File.Exists(_fileName)) 
        {
            _logger.LogError("Failed to load or parse prompts from JSON.");
            throw new FileNotFoundException($"The file {_fileName} was not found.");
        }
        
        var prompts = GetPromptsFromJson();
        if(prompts is null) throw new Exception("An internal server error occurred.");
        
        //System Messages
        List<ChatMessage> messages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, prompts.SystemPrompt),
        };

        //Keyword Prompt
        if (!keywordTypes.Any()) keywordTypes = new[] { KeywordType.Lsi, KeywordType.Longtail, KeywordType.Related, KeywordType.Short, KeywordType.QuestionBased };
        string keywordPrompt = GenerateKeywordPrompt(keywordTypes, prompts.KeywordPrompt, "{Placeholder}"); //Generate prompt for keywords with all keywordtypes
        messages.Add(new ChatMessage(ChatMessageRole.User, keywordPrompt));
        
        //Predefined Prompts from Json
        messages.AddRange(prompts.Prompts.Select(prompt => new ChatMessage(ChatMessageRole.User, prompt)));

        //Prompts from User/Input content
        if (contentPrompt is not null) messages.Add(new ChatMessage(ChatMessageRole.User, contentPrompt));

        return messages;
    }

    private string GenerateKeywordPrompt(KeywordType[] keywordTypes, string keywordPrompt, string placeholder)
    {
        string keywords = "";
        foreach (var (value, i) in keywordTypes.Select((value, i) => ( value, i )))
        {
            keywords = keywords + $"{value.ToString()}{(i==keywordTypes.Length-1 ? ' ' : ", ")}"; //last KeywordType no comma
        }

        return keywordPrompt.Replace(placeholder, keywords);
    }

    private PromptsStructure? GetPromptsFromJson()
    {
        try
        {
            string json = File.ReadAllText(_fileName);
            return JsonSerializer.Deserialize<PromptsStructure>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("An error occurred while deserializing the JSON.", ex);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while reading prompts from JSON.");
            throw new Exception("An internal server error occurred.", e);
        }
        
    }
}