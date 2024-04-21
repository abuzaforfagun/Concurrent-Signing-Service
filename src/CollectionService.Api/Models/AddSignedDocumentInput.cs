namespace CollectionService.Api.Models;

public class AddSignedDocumentInput
{
    public Guid DocumentId { get; set; }
    public string Content { get; set; }
}