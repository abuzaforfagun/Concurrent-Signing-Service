namespace MessageProcessor.Handlers;

public interface ISigningTriggeredHandler
{
    Task StartProcessingAsync(CancellationToken token);
    Task StopProcessingAsync();
}