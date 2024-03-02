namespace SEO_Optimizer.Models;

public class PromptsStructure
{
    public required string SystemPrompt { get; set; }
    public required string KeywordPrompt { get; set; }
    public required IList<string> Prompts { get; set; }
}