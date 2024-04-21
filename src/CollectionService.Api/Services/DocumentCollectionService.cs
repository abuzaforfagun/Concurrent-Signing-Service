using System.Data;
using CollectionService.Api.Config;
using CollectionService.Api.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CollectionService.Api.Services;

public interface IDocumentCollectionService
{
    Task<List<UnSignedDocument>> GetAsync(int take, int skip);
    Task<int> GetNumberOfUnSignedDocuments();
    Task<AddSignedDocumentsResult> AddSignedDocumentAsync(List<SignedDocument> documents);
}
public class DocumentCollectionService : IDocumentCollectionService
{
    private readonly DatabaseOptions _databaseConfig;

    public DocumentCollectionService(IOptions<DatabaseOptions> databaseConfig)
    {
        _databaseConfig = databaseConfig.Value;
    }

    public async Task<List<UnSignedDocument>> GetAsync(int take, int skip)
    {
        using IDbConnection connection = new SqlConnection(_databaseConfig.PublicDataConnectionString);
        connection.Open();  

        var query = @"
SELECT Id, Content, CreatedAtUtc
FROM UnsignedDocuments
ORDER BY CreatedAtUtc
OFFSET @Skip ROWS
FETCH NEXT @Take ROWS ONLY;";

        var data = await connection.QueryAsync<UnSignedDocument>(query, new {Take = take, Skip = skip});
        return data.ToList();
    }

    public async Task<int> GetNumberOfUnSignedDocuments()
    {
        using IDbConnection connection = new SqlConnection(_databaseConfig.PublicDataConnectionString);
        connection.Open();

        return await connection.QuerySingleOrDefaultAsync<int>("SELECT COUNT(1) FROM UnsignedDocuments");
    }

    public async Task<AddSignedDocumentsResult> AddSignedDocumentAsync(List<SignedDocument> documents)
    {
        using IDbConnection dbConnection = new SqlConnection(_databaseConfig.PublicDataConnectionString);
        dbConnection.Open();

        var documentIds = documents.Select(d => d.DocumentId);
        var existingDocuments = (await dbConnection.QueryAsync<int>(
            "SELECT DocumentId FROM SignedDocuments WHERE DocumentId IN (@DocumentId)", new
            {
                DocumentId = documentIds
            })).ToList();

        if (existingDocuments.Any())
        {
            return new AddSignedDocumentsResult(existingDocuments);
        }

        var insertQuery = "INSERT INTO SignedDocuments (DocumentId, Content) VALUES (@DocumentId, @Content)";

        try
        {
            var affectedRecords = await dbConnection.ExecuteAsync(insertQuery, documents);

            return new AddSignedDocumentsResult(affectedRecords > 0);
        }
        catch (Exception)
        {
            return new AddSignedDocumentsResult(false);
        }
    }
}