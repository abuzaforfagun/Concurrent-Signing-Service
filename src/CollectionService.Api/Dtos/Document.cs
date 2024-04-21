namespace CollectionService.Api.Dtos;

public class Document
{
    public Guid Id { get; set; }
    public string Content { get; set; } = default!;
}
