using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using SEO_Optimizer.Controllers;
using SEO_Optimizer.Interfaces;
using SEO_Optimizer.Models;
using SEO_Optimizer.Utils;

namespace SEO_OptimizerTests;

[TestFixture]
public class Tests
{
    [TestFixture]
    public class SeoControllerTests
    {
        private Mock<IOpenAiInterface> _openAiServiceMock;
        private Mock<ILogger<SeoController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _openAiServiceMock = new Mock<IOpenAiInterface>();
            _loggerMock = new Mock<ILogger<SeoController>>();
        }

        [Test]
        public async Task SeoOptimizing_ShouldReturnBadRequest_WhenInputIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PromptBuilder>>();
            var promptBuilder = new PromptBuilder(loggerMock.Object);

            var controller = new SeoController(_openAiServiceMock.Object, _loggerMock.Object, promptBuilder);
            // ..
            
            SeoInput input = null;

            // Act
            var result = await controller.SeoOptimizing(input);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task SeoOptimizing_ShouldReturnOkResult_WhenInputIsValid()
        {
            // Arrange
            var input = new SeoInput { Content = "Test Content", types = new [] { KeywordType.Lsi }};
            var chatResult = new ChatResult { Model = OpenAI_API.Models.Model.GPT4_Turbo};
            _openAiServiceMock.Setup(s => s.SendChatMessages(It.IsAny<List<ChatMessage>>())).ReturnsAsync(chatResult);

            var promptBuilderLoggerMock = new Mock<ILogger<PromptBuilder>>();
            var promptBuilder = new PromptBuilder(promptBuilderLoggerMock.Object);
            var controller = new SeoController(_openAiServiceMock.Object, _loggerMock.Object, promptBuilder);

            // Act
            var result = await controller.SeoOptimizing(input);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(200, objectResult.StatusCode); // OK Status Code is 200
        }
        
        [Test]
        public async Task SeoOptimizing_ShouldReturnBadRequest_WhenOpenAiServiceThrowsError()
        {
            // Arrange
            var input = new SeoInput { Content = "Test Content", types = new [] { KeywordType.Lsi }};
            _openAiServiceMock.Setup(s => s.SendChatMessages(It.IsAny<List<ChatMessage>>())).ThrowsAsync(new Exception("OpenAI Service Error"));

            var promptBuilderLoggerMock = new Mock<ILogger<PromptBuilder>>();
            var promptBuilder = new PromptBuilder(promptBuilderLoggerMock.Object);
            var controller = new SeoController(_openAiServiceMock.Object, _loggerMock.Object, promptBuilder);

            // Act
            var result = await controller.SeoOptimizing(input);
            
            //Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
        [Test]
        public async Task SeoOptimizing_ShouldReturnBadRequest_WhenInputIsEmpty()
        {
            // Arrange
            SeoInput input = new SeoInput() {Content = "", types = Array.Empty<KeywordType>()}; // Leere Eingabe
            var promptBuilderLoggerMock = new Mock<ILogger<PromptBuilder>>();
            var promptBuilder = new PromptBuilder(promptBuilderLoggerMock.Object);
            var controller = new SeoController(_openAiServiceMock.Object, _loggerMock.Object, promptBuilder);

            // Act
            var result = await controller.SeoOptimizing(input);

            //Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);
        }
    }
}