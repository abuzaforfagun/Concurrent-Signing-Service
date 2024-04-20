namespace SigningService.Api.Models;

public class SigningOutput
{
    public Guid Id { get; set; }
    public string SignedData { get; set; } = default!;
}