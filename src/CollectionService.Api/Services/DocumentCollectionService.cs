using System.Data;
using CollectionService.Api.Config;
using CollectionService.Api.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CollectionService.Api.Services;

public interface IDocumentCollectionService
{
    Task<List<Document>> GetAsync(int take, int skip);
}
public class DocumentCollectionService : IDocumentCollectionService
{
    private readonly DatabaseOptions _databaseConfig;

    public DocumentCollectionService(IOptions<DatabaseOptions> databaseConfig)
    {
        _databaseConfig = databaseConfig.Value;
    }

    public async Task<List<Document>> GetAsync(int take, int skip)
    {
        using IDbConnection connection = new SqlConnection(_databaseConfig.PublicDataConnectionString);
        connection.Open();  

        var query = @"
SELECT Id, Content, CreatedAtUtc
FROM UnsignedDocuments
ORDER BY CreatedAtUtc
OFFSET @Skip ROWS
FETCH NEXT @Take ROWS ONLY;";

        var data = await connection.QueryAsync<Document>(query, new {Take = take, Skip = skip});
        return data.ToList();
    }
}