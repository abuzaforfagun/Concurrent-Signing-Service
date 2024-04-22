using System.Data;
using Dapper;
using KeyManagement.Api.Config;
using KeyManagement.Api.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace KeyManagement.Api.Services
{
    public interface IKeyStorageService
    {
        Task<Guid?> PopLeastUsedKeyAsync();
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
        public async Task<Guid?> PopLeastUsedKeyAsync()
        {
            using IDbConnection connection = new SqlConnection(_databaseConfig.KeyStoreConnectionString);
            connection.Open();

            Guid? result = null;
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                // Execute Dapper query within the transaction
                result = await connection.QuerySingleOrDefaultAsync<Guid?>(
                    "SELECT TOP 1 Id FROM Keys WITH (ROWLOCK, XLOCK) WHERE IsLocked=0",
                    transaction: transaction);

                if (result is null)
                {
                    transaction.Rollback();
                    return null;
                }

                var updateQuery = "UPDATE Keys SET IsLocked=1 WHERE Id = @Id";
                await connection.ExecuteAsync(updateQuery, new
                {
                    Id = result
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

            var privateKey = await connection.QuerySingleOrDefaultAsync<string>("SELECT PrivateKey FROM Keys WHERE Id=@Id AND IsLocked=0", new
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

            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                await connection.ExecuteAsync("DELETE FROM Keys WHERE Id=@Id", new
                {
                    Id = id
                }, transaction);

                await connection.ExecuteAsync(
                    "INSERT INTO Keys (Id, PublicKey, PrivateKey) VALUES (@Id, @PublicKey, @PrivateKey)", new
                    {
                        Id = key.Id,
                        PublicKey = key.PublicKey,
                        PrivateKey = key.PrivateKey
                    }, transaction);
            }
            catch (Exception)
            {
                transaction.Rollback();

                return false;
            }
            

            return true;
        }
    }
}
