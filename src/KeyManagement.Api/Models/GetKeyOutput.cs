namespace KeyManagement.Api.Models;

public class GetKeyOutput
{
    public Guid Id { get; set; }
    public string PrivateKey { get; set; } = default!;
}