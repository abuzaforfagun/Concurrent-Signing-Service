using System.Data;
using Dapper;
using DataSeeder.Config;
using DataSeeder.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace DataSeeder.Services;

public class PublicDataSeeder : IPublicDataSeeder
{
    private readonly DatabaseOptions _databaseOptions;

    public PublicDataSeeder(IOptions<DatabaseOptions> databaseOptions)
    {
        _databaseOptions = databaseOptions.Value;
    }
    public async Task SeedDataAsync(int numberOfData)
    {
        using IDbConnection dbConnection = new SqlConnection(_databaseOptions.PublicDataConnectionString);
        dbConnection.Open();
        var insertQuery = "INSERT INTO UnsignedDocuments (Content) VALUES (@Content)";

        var records = new List<PublicDocument>();
        for (var i = 0; i < numberOfData; i++)
        {
            var content = $"Document No {i + 1}";
            records.Add(new PublicDocument
            {
                Content = content
            });
        }

        await dbConnection.ExecuteAsync(insertQuery, records);
    }

    public async Task<bool> HasData()
    {
        using IDbConnection dbConnection = new SqlConnection(_databaseOptions.PublicDataConnectionString);
        dbConnection.Open();
        var query = "SELECT TOP 1 * FROM UnsignedDocuments";

        var data = await dbConnection.QuerySingleOrDefaultAsync<PublicDocument>(query);

        return data != null;
    }
}