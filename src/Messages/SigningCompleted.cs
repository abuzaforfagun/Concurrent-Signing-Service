namespace Messages;

public class SigningCompleted : IMessage
{
    public const string QueueName = "signing-completed";
    public Guid MessageId { get; set; }
    public DateTimeOffset TriggeredAtUtc { get; set; }
    public string KeyId { get; set; } = default!;

}