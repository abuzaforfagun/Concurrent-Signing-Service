namespace CollectionService.Api.Models;

public class GetDocumentOutput<T>
{
    public List<T> Documents { get; } = new();
    public int PageNumber { get; }
    public int PageSize { get; }
    public int Count { get; }

    public GetDocumentOutput(List<T> documents, int pageNumber, int pageSize)
    {
        Documents = documents;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Count = documents.Count;
    }
}