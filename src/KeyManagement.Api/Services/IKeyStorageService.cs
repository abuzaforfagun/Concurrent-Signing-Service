using KeyManagement.Api.Dtos;

namespace KeyManagement.Api.Services;

public interface IKeyStorageService
{
    Task<Key?> PopLeastUsedKeyAsync();
    Task<string?> GetPrivateKeyAsync(Guid id);
    Task<bool> ReleaseLockAsync(Guid id);
}