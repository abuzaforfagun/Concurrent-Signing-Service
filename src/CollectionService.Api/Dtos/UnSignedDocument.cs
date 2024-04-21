namespace CollectionService.Api.Dtos;

public class UnSignedDocument
{
    public Guid Id { get; set; }
    public string Content { get; set; } = default!;
}

public class SignedDocument
{
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = default!;
}
