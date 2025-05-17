namespace Briefcase.Infrastructure.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using System.Collections.Generic;
using Azure.AI.DocumentIntelligence;
using Briefcase.Core.Interfaces;
using Briefcase.Core.Models;
using Azure.Identity;

public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _documentClient;

    public DocumentIntelligenceService(IKeyVaultService keyVaultService)
    {
        var connectionString = keyVaultService.GetSecret("briefcaseocr-url");
        _documentClient = new DocumentIntelligenceClient(new Uri(connectionString), new DefaultAzureCredential());
    }

    public async Task<string> ExtractTextAsync(Stream documentStream)
    {
        var options = new AnalyzeDocumentOptions("prebuilt-read", new BinaryData(documentStream));
        var operation = await _documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, options);
        var result = operation.Value;

        return result.Content;
    }

    public async Task<KeyValuePairResult> ExtractKeyValuePairsAsync(Stream documentStream)
    {
        var options = new AnalyzeDocumentOptions("prebuilt-document", new BinaryData(documentStream));
        var operation = await _documentClient.AnalyzeDocumentAsync(WaitUntil.Completed,options);
        var result = operation.Value;

        var keyValuePairs = new List<KeyValuePair<string, string>>();

        foreach(var kvp in result.KeyValuePairs)
        {
            var key = kvp.Key?.Content?.Trim() ?? string.Empty;
            var value = kvp.Value?.Content?.Trim() ?? string.Empty;

            if(!string.IsNullOrWhiteSpace(key))
            {
                keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        return new KeyValuePairResult { Pairs = keyValuePairs };
    }

    public async Task<AnalyzeResult> AnalyzeWithCustomModelAsync(Stream documentStream, string modelId)
    {
        var options = new AnalyzeDocumentOptions(modelId, new BinaryData(documentStream));
        var operation = await _documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, options);
        return operation.Value;
    }
}

