namespace CollectionService.Api.Dtos;

public class UnsignedDocument
{
    public Guid Id { get; set; }
    public string Document { get; set; } = default!;
}
