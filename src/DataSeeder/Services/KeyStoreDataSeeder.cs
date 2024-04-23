using System.Data;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DataSeeder.Config;
using DataSeeder.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace DataSeeder.Services;

public class KeyStoreDataSeeder : IKeyStoreDataSeeder
{
    private readonly DatabaseOptions _databaseOptions;

    public KeyStoreDataSeeder(IOptions<DatabaseOptions> databaseOptions)
    {
        _databaseOptions = databaseOptions.Value;
    }
    public async Task SeedDataAsync(int numberOfData)
    {
        using IDbConnection dbConnection = new SqlConnection(_databaseOptions.KeyStoreConnectionString);
        dbConnection.Open();
        var insertQuery = "INSERT INTO Keys (PublicKey, PrivateKey) VALUES (@PublicKey, @PrivateKey)";

        var records = new List<Key>();
        for (var i = 0; i < numberOfData; i++)
        {
            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            // Convert the parameters to XML strings
            string publicKeyXml = rsa.ToXmlString(false);
            string privateKeyXml = rsa.ToXmlString(true);

            records.Add(new Key
            {
                PublicKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKeyXml)),
                PrivateKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKeyXml))
            });
        }

        await dbConnection.ExecuteAsync(insertQuery, records);
    }

    public async Task<bool> HasData()
    {
        using IDbConnection dbConnection = new SqlConnection(_databaseOptions.KeyStoreConnectionString);
        dbConnection.Open();
        var query = "SELECT TOP 1 * FROM Keys";

        var data = await dbConnection.QuerySingleOrDefaultAsync<Key>(query);

        return data != null;
    }
}