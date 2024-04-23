using System.Data;
using Dapper;
using KeyManagement.Api.Config;
using KeyManagement.Api.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace KeyManagement.Api.Services;

public interface IKeyStorageService
{
    Task<Key?> PopLeastUsedKeyAsync();
    Task<string?> GetPrivateKeyAsync(Guid id);
    Task<bool> ReleaseLockAsync(Guid id);
}

public class KeyStorageService : IKeyStorageService
{
    private readonly DatabaseOptions _databaseConfig;

    public KeyStorageService(IOptions<DatabaseOptions> databaseConfig)
    {
        _databaseConfig = databaseConfig.Value;
    }
    public async Task<Key?> PopLeastUsedKeyAsync()
    {
        using IDbConnection connection = new SqlConnection(_databaseConfig.KeyStoreConnectionString);
        connection.Open();

        Key? result = null;
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            // Execute Dapper query within the transaction
            result = await connection.QuerySingleOrDefaultAsync<Key>(
                "SELECT TOP 1 Id, PrivateKey FROM Keys WITH (ROWLOCK, XLOCK) WHERE IsLocked=0 ORDER BY ModifiedAtUtc ASC",
                transaction: transaction);

            if (result is null)
            {
                transaction.Rollback();
                return null;
            }

            var updateQuery = "UPDATE Keys SET IsLocked=1 WHERE Id = @Id";
            await connection.ExecuteAsync(updateQuery, new
            {
                Id = result.Id
            }, transaction);
                
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            return null;
        }

        return result;
    }

    public async Task<string?> GetPrivateKeyAsync(Guid id)
    {
        using IDbConnection connection = new SqlConnection(_databaseConfig.KeyStoreConnectionString);
        connection.Open();

        var privateKey = await connection.QuerySingleOrDefaultAsync<string>("SELECT PrivateKey FROM Keys WHERE Id=@Id", new
        {
            Id = id
        });

        return privateKey;
    }

    public async Task<bool> ReleaseLockAsync(Guid id)
    {
        using IDbConnection connection = new SqlConnection(_databaseConfig.KeyStoreConnectionString);
        connection.Open();

        var key = await connection.QuerySingleOrDefaultAsync<Key>("SELECT * FROM Keys WHERE Id=@Id AND IsLocked=1", new
        {
            Id = id
        });

        if (key is null)
        {
            return false;
        }

        await connection.ExecuteAsync(
            "UPDATE Keys SET IsLocked=0, ModifiedAtUtc=@CurrentTime WHERE Id=@Id", new
            {
                Id = id,
                CurrentTime = DateTimeOffset.UtcNow
            });

        return true;
    }
}