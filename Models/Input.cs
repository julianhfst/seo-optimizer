namespace SEO_Optimizer.Models;

public class SeoInput
{
    public required string Content { get; set; }
    public required KeywordType[] types { get; set; }
}