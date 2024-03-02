using Microsoft.Extensions.Logging;
using Moq;
using OpenAI_API.Chat;
using SEO_Optimizer.Controllers;
using SEO_Optimizer.Interfaces;
using SEO_Optimizer.Models;
using SEO_Optimizer.Utils;

namespace SEO_OptimizerTests;

[TestFixture]
public class PromptBuilderTests
{
    private Mock<ILogger<PromptBuilder>> loggerMock;
    
    [SetUp]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<PromptBuilder>>();
    }
    
    [Test]
    public void BuildPrompts_ShouldCreateChatMessages()
    {
        // Arrange
        var keywordTypes = new KeywordType[] { KeywordType.Lsi };
        var contentPrompt = "Test Content Prompt";
        var promptBuilder = new PromptBuilder(loggerMock.Object);

        // Act
        var chatMessages = promptBuilder.BuildPrompts(keywordTypes, contentPrompt);

        // Assert
        Assert.IsNotNull(chatMessages);
        var keywordPromptMessage = chatMessages.FirstOrDefault(msg => msg.Role.Equals(ChatMessageRole.User));
        Assert.IsNotNull(keywordPromptMessage);
        foreach (var keywordType in keywordTypes)
        {
            Assert.IsTrue(keywordPromptMessage.Content.Contains(keywordType.ToString()));
        }
        var systemPromptMessage = chatMessages.FirstOrDefault(msg => msg.Role.Equals(ChatMessageRole.User));
        Assert.IsNotNull(systemPromptMessage);
    }
    
}