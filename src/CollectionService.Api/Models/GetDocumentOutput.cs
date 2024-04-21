namespace CollectionService.Api.Models;

public class GetDocumentOutput<T>
{
    public List<T> Documents { get; } = new();
    public int PageNumber { get; }
    public int PageSize { get; }
    public int Count { get; }
    public int TotalItems { get; }

    public GetDocumentOutput(List<T> documents, int pageNumber, int pageSize, int totalItems)
    {
        Documents = documents;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Count = documents.Count;
        TotalItems = totalItems;
    }
}