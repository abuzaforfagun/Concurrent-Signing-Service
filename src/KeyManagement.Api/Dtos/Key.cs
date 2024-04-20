namespace KeyManagement.Api.Dtos;

public class Key
{
    public Guid Id { get; set; }
    public string PublicKey { get; set; } = default!;
    public string PrivateKey { get; set; } = default!;
}