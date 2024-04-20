namespace KeyManagement.Api.Models;

public class GetPublicKeyOutput
{
    public Guid Id { get; set; }
    public string PublicKey { get; set; } = default!;
}