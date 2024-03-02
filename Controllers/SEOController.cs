using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SEO_Optimizer.Interfaces;
using SEO_Optimizer.Models;
using SEO_Optimizer.Utils;

namespace SEO_Optimizer.Controllers;

[ApiController]
[Route("")]
public class SeoController : Controller
{
    private readonly IOpenAiInterface _openAiService;
    private readonly PromptBuilder _promptBuilder;
    private readonly ILogger<SeoController> _logger;
    
    public SeoController(IOpenAiInterface openAiService, ILogger<SeoController> logger, PromptBuilder promptBuilder)
    {
        _openAiService = openAiService;
        _logger = logger;
        _promptBuilder = promptBuilder;
    }
    
    [HttpPost]
    [Route("/api")]
    public async Task<IActionResult> SeoOptimizing([FromBody] SeoInput input)
    {
        if (input is null || string.IsNullOrEmpty(input.Content))
        {
            _logger.LogWarning("Received bad request with empty input.");
            return BadRequest("Input content is required.");
        }

        try
        {
            var messages = _promptBuilder.BuildPrompts(input.types, input.Content);
            var apiResult = await _openAiService.SendChatMessages(messages);
            
            _logger.LogInformation("Received API result: {apiResult}", JsonSerializer.Serialize(apiResult));

            var json = apiResult.ToString();
            SeoOutput? output = JsonSerializer.Deserialize<SeoOutput>(json);
            
            if (output is null) 
            {
                _logger.LogError("Failed to deserialize API result to SeoOutput.");
                return StatusCode(500, "An internal server error occurred.");
            }
        
            return Ok(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }
    
    [HttpPost]
    [Route("/output")]
    public async Task<IActionResult> OutputView([FromBody] SeoInput input)
    {
        if (input == null || string.IsNullOrEmpty(input.Content))
        {
            _logger.LogWarning("Received bad request with empty input.");
            return BadRequest("Input content is required.");
        }

        try
        {
            var messages = _promptBuilder.BuildPrompts(input.types, input.Content);
            var apiResult = await _openAiService.SendChatMessages(messages);
            
            _logger.LogInformation("Received API result: {apiResult}", JsonSerializer.Serialize(apiResult));

            var json = apiResult.ToString();
            SeoOutput? output = JsonSerializer.Deserialize<SeoOutput>(json);
            if (output is null)
            {
                _logger.LogError("Failed to deserialize API result to SeoOutput.");
                return new ContentResult {
                    ContentType = "text/html",
                    StatusCode = (int) HttpStatusCode.InternalServerError,
                    Content = this.ErrorHtml(500)
                };
            }
            
            var html = await System.IO.File.ReadAllTextAsync("./wwwroot/output.html");
            html = html.Replace("{Title}", output.Title)
                .Replace("{Keywords}", output.Keywords)
                .Replace("{MetaDescription}", output.MetaDescription)
                .Replace("{Result}", output.Result);
            
            return new ContentResult {
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.OK,
                Content = html
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return new ContentResult {
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.InternalServerError,
                Content = this.ErrorHtml(500)
            };
        }
    }

    private string ErrorHtml(int  code)
    {
        var html = System.IO.File.ReadAllText("./wwwroot/error.html");
        html = html.Replace("{Code}", code.ToString());
        return html;
    }
}