using System.Text;
using Azure.Messaging.ServiceBus;
using CollectionService.Api.Client;
using KeyManagement.Api.Client;
using MessageProcessor.Config;
using MessageProcessor.Infrastructure;
using Messages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SigningService.Api.Client;

namespace MessageProcessor.Handlers;

public interface ISigningCompletedHandler
{
    Task StartProcessingAsync(CancellationToken token);
    Task StopProcessingAsync();
}

public class SigningCompletedHandler : ISigningCompletedHandler
{
    private readonly IKeysClient _keysClient;
    private readonly ServiceBusClient _serviceBusClient;

    readonly string QueueName = SigningTriggered.QueueName;

    public SigningCompletedHandler(
        IServiceBusClientFactory serviceBusClientFactory, 
        IKeysClient keysClient)
    {
        _keysClient = keysClient;
        _serviceBusClient = serviceBusClientFactory.CreateClient();
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
        await _serviceBusClient.DisposeAsync();
    }

    async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var messageJson = Encoding.UTF8.GetString(message.Body);
        var payload = JsonConvert.DeserializeObject<SigningCompleted>(messageJson);

        await _keysClient.ReleaseLockAsync(new Guid(payload!.KeyId));
        await args.CompleteMessageAsync(message);
    }

    Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception}");
        return Task.CompletedTask;
    }
}