using Briefcase.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Briefcase.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentIntelligenceController(IDocumentIntelligenceService documentIntelligenceService) : ControllerBase
{
    private readonly IDocumentIntelligenceService _documentIntelligenceService = documentIntelligenceService;
    [HttpPost("extract-text")]
    public async Task<IActionResult> ExtractText(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var result = await _documentIntelligenceService.ExtractTextAsync(memoryStream);
        return Ok(result);
    }
    [HttpPost("custom/{modelId}")]
    public async Task<IActionResult> AnalyzeWithCustomModel(IFormFile file, string modelId)
    {
        if(file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        var result = await _documentIntelligenceService.ClassifyDocument(memoryStream);
        return Ok(result);
    }
}
