using Azure.AI.DocumentIntelligence;
using Briefcase.Core.Models;
namespace Briefcase.Core.Interfaces;
public interface IDocumentIntelligenceService
{
    Task<string> ExtractTextAsync(string blobUrl);
    Task<string> ExtractTextAsync(Stream documentStream);
    Task<KeyValuePairResult> ExtractKeyValuePairsAsync(Stream documentStream);
    Task<AnalyzeResult> AnalyzeWithCustomModelAsync(Stream documentStream, string modelId);
    Task<AnalyzeResult> ClassifyDocument(Stream stream);
}
