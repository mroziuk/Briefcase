using Briefcase.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Briefcase.API.Controllers;
[ApiController]
[Route("[controller]")]
public class FileController(IBlobStorageService blobStorageService, IDocumentIntelligenceService documentIntelligenceService) : ControllerBase

{
    private readonly IBlobStorageService _blobStorageService = blobStorageService;
    private readonly IDocumentIntelligenceService _documentIntelligenceService = documentIntelligenceService;
    [HttpPost]
    public async Task<ActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
        var containerName = "container-01"; 
        var blobName = file.FileName;
        using (var stream = file.OpenReadStream())
        {
            await _blobStorageService.UploadAsync(containerName, blobName, stream, file.ContentType);
        }
        var blobUrl = $"{_blobStorageService.GetBlobServiceUri()}/{containerName}/{blobName}";
        //return Ok(new { BlobUrl = blobUrl, BlobName = blobName });
        var text = await _documentIntelligenceService.ExtractTextAsync(blobUrl);
        return Ok(new { BlobUrl = blobUrl, BlobName = blobName, Text = text });
    }
}
