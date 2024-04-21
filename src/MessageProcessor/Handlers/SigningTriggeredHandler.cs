using System.Text;
using Azure.Messaging.ServiceBus;
using MessageProcessor.Infrastructure;
using Messages;

namespace MessageProcessor.Handlers;

public interface ISigningTriggeredHandler
{
    Task StartProcessingAsync(CancellationToken token);
    Task StopProcessingAsync();
}

public class SigningTriggeredHandler : ISigningTriggeredHandler
{
    private readonly IServiceBusClientFactory _serviceBusClientFactory;
    private ServiceBusClient _serviceBusClient;

    readonly string QueueName = SigningTriggered.QueueName;

    public SigningTriggeredHandler(IServiceBusClientFactory serviceBusClientFactory)
    {
        _serviceBusClientFactory = serviceBusClientFactory;
        _serviceBusClient = _serviceBusClientFactory.CreateClient();
    }

    public async Task StartProcessingAsync(CancellationToken token)
    {
        var processor = _serviceBusClient.CreateProcessor(QueueName);

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync(token);
    }

    public async Task StopProcessingAsync()
    {
        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync();
        }
    }

    async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        Console.WriteLine($"Received message: SequenceNumber:{message.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

        // Simulate processing time
        await Task.Delay(1000);

        await args.CompleteMessageAsync(message);
    }

    Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception}");
        return Task.CompletedTask;
    }
}