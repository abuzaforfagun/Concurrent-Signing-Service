namespace CollectionService.Api.Dtos;

public class AddSignedDocumentsResult
{
    public bool IsSuccess { get; }
    public List<int> ExistingDocumentIds { get; } = new();

    public AddSignedDocumentsResult(List<int> existingDocumentIds)
    {
        ExistingDocumentIds = existingDocumentIds;
        IsSuccess = existingDocumentIds.Count == 0;
    }

    public AddSignedDocumentsResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }
}