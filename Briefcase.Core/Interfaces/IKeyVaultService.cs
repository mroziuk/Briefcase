namespace Briefcase.Core.Interfaces;
public interface IKeyVaultService
{
    Task SetSecretAsync(string name, string value);
    Task<string> GetSecretAsync(string name);
    string GetSecret(string name);
    Task<bool> DeleteSecretAsync(string name);
    Task<bool> SecretExistsAsync(string name);
}
