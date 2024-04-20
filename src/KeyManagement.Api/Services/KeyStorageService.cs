﻿using System.Data;
using Dapper;
using KeyManagement.Api.Config;
using KeyManagement.Api.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace KeyManagement.Api.Services
{
    public interface IKeyStorageService
    {
        Task<(Guid?, string?)> PopLeastUsedKeyAsync();
        Task<bool> ReleaseLockAsync(Guid id);
    }

    public class KeyStorageService : IKeyStorageService
    {
        private readonly DatabaseOptions _databaseConfig;

        public KeyStorageService(IOptions<DatabaseOptions> databaseConfig)
        {
            _databaseConfig = databaseConfig.Value;
        }
        public async Task<(Guid?, string?)> PopLeastUsedKeyAsync()
        {
            using IDbConnection connection = new SqlConnection(_databaseConfig.KeyStoreConnectionString);
            connection.Open();

            Key? result = null;
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                // Execute Dapper query within the transaction
                result = await connection.QuerySingleOrDefaultAsync<Key>(
                    "SELECT TOP 1 Id, PublicKey FROM Keys WITH (ROWLOCK, XLOCK) WHERE IsLocked=0",
                    transaction: transaction);

                if (result is null)
                {
                    transaction.Rollback();
                    return (null, null);
                }

                var updateQuery = "UPDATE Keys SET IsLocked=1 WHERE Id = @Id";
                await connection.ExecuteAsync(updateQuery, new
                {
                    Id = result!.Id
                }, transaction);
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return (null, null);
            }

            return (result!.Id, result.PublicKey);
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

            await connection.ExecuteAsync("DELETE FROM Keys WHERE Id=@Id", new
            {
                Id = id
            });

            await connection.ExecuteAsync("INSERT INTO Keys (Id, PublicKey, PrivateKey) VALUES (@Id, @PublicKey, @PrivateKey)", new
            {
                Id = key.Id,
                PublicKey = key.PublicKey,
                PrivateKey = key.PrivateKey
            });

            return true;
        }
    }
}
