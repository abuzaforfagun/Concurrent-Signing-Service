namespace CollectionService.Api.Dtos;

public class AddSignedDocumentsResult
{
    public bool IsSuccess { get; }
    public List<Guid> ExistingDocumentIds { get; } = new();

    public AddSignedDocumentsResult(List<Guid> existingDocumentIds)
    {
        ExistingDocumentIds = existingDocumentIds;
        IsSuccess = existingDocumentIds.Count == 0;
    }

    public AddSignedDocumentsResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }
}