using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Briefcase.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Briefcase.Infrastructure.Services;
public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;

    public KeyVaultService(IConfiguration configuration)
    {
        var keyVaultUrl = configuration["AzureKeyVault:Url"];
        ArgumentNullException.ThrowIfNull(nameof(keyVaultUrl));
        _secretClient = new SecretClient(new Uri(keyVaultUrl!), new DefaultAzureCredential());
    }

    public async Task SetSecretAsync(string name, string value)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Secret name cannot be null or empty.", nameof(name));

        await _secretClient.SetSecretAsync(name, value);
    }

    public async Task<string> GetSecretAsync(string name)
    {
        try
        {
            var response = await _secretClient.GetSecretAsync(name);
            return response.Value.Value;
        }
        catch(RequestFailedException ex) when(ex.Status == 404)
        {
            throw new KeyNotFoundException($"Secret '{name}' not found in Key Vault.");
        }
    }

    public async Task<bool> DeleteSecretAsync(string name)
    {
        try
        {
            await _secretClient.StartDeleteSecretAsync(name);
            return true;
        }
        catch(RequestFailedException ex) when(ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<bool> SecretExistsAsync(string name)
    {
        try
        {
            var _ = await _secretClient.GetSecretAsync(name);
            return true;
        }
        catch(RequestFailedException ex) when(ex.Status == 404)
        {
            return false;
        }
    }

    public string GetSecret(string name)
    {
        return _secretClient.GetSecret(name).Value.Value;
    }
}