using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace KeyManagement.Api.IntegrationTests.Infrastructure;

public static class DbHelper
{
    public static async Task CleanUpDatabaseAsync()
    {
        const string sql = "DELETE FROM Keys;";
        using IDbConnection connection = new SqlConnection(TestStaticSettings.KeyStoreDbConnectionString);
        connection.Open();

        await connection.ExecuteAsync(sql);
    }

    public static async Task AddNewKeyAsync(string privateKey, string publicKey, DateTimeOffset? modifiedAtUtc = null, bool isLocked = false)
    {
        if (modifiedAtUtc is null)
        {
            modifiedAtUtc = DateTimeOffset.UtcNow;
        }
        const string sql = "INSERT INTO Keys (PrivateKey, PublicKey, IsLocked, ModifiedAtUtc) VALUES (@PrivateKey, @PublicKey, @IsLocked, @ModifiedAtUtc)";
        using IDbConnection connection = new SqlConnection(TestStaticSettings.KeyStoreDbConnectionString);
        connection.Open();

        await connection.ExecuteAsync(sql, new
        {
            PrivateKey = privateKey,
            PublicKey = publicKey,
            IsLocked = isLocked,
            ModifiedAtUtc = modifiedAtUtc
        });
    }

    public static async Task<bool> GetKeyIsLockedStatusAsync(Guid keyId)
    {
        const string sql = "SELECT IsLocked FROM Keys WHERE Id = @Id";
        using IDbConnection connection = new SqlConnection(TestStaticSettings.KeyStoreDbConnectionString);
        connection.Open();

        return await connection.QuerySingleOrDefaultAsync<bool>(sql, new
        {
            Id = keyId
        });
    }
}