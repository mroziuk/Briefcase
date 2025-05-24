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
using Microsoft.Identity.Client;

public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _documentClient;

    public DocumentIntelligenceService(IKeyVaultService keyVaultService)
    {
        string endpoint = keyVaultService.GetSecret("briefcaseocr-url");
        string key = keyVaultService.GetSecret("briefcaseocr-key1");
        _documentClient = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(key));
    }
    public async Task<string> ExtractTextAsync(string blobUrl)
    {
        var options = new AnalyzeDocumentOptions("prebuilt-read", new Uri(blobUrl));
        var operation = await _documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, options);
        return operation.Value.Content;
    }
    public async Task<string> ExtractTextAsync(Stream documentStream)
    {
        var byteArray = new BinaryData(GetBytes(documentStream));
        var options = new AnalyzeDocumentOptions("prebuilt-read", byteArray);
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
        var options = new AnalyzeDocumentOptions(modelId, new BinaryData(GetBytes(documentStream)));
        var operation = await _documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, options);
        return operation.Value;
    }
    public async Task<AnalyzeResult> ClassifyDocument(Stream stream)
    {
        var options = new ClassifyDocumentOptions("food-trip-document", new BinaryData(GetBytes(stream)));
        var operation = await _documentClient.ClassifyDocumentAsync(WaitUntil.Completed, options);
        var result = operation.Value;
        return result;
    }
    private static byte[] GetBytes(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}

