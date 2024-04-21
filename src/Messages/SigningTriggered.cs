namespace Messages;

public class SigningTriggered : IMessage
{
    public const string QueueName = "signing-triggered";
    public Guid MessageId { get; set; }
    public DateTimeOffset TriggeredAtUtc { get; set; }
    public string PublicKey { get; set; } = default!;
    public List<Document> Documents { get; set; } = new();
    
}

public class Document
{
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = default!;
}