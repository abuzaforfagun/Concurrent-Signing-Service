using System.ComponentModel.DataAnnotations;

namespace SigningService.Api.Models;

public class SigningInput
{
    [Required]
    public string PublicKey { get; set; } = default!;

    [MaxLength(1000)]
    public List<DataItem> Data { get; set; } = new();
}

public class DataItem
{
    public Guid Id { get; set; }
    public string Content { get; set; } = default!;
}