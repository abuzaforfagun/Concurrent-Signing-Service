namespace Messages;

public interface IMessage
{
    public Guid MessageId { get; set; }
    public DateTimeOffset TriggeredAtUtc { get; set; }
}