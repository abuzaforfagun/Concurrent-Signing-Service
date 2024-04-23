using System.Text;
using Azure.Messaging.ServiceBus;
using CollectionService.Api.Client;
using MessageProcessor.Config;
using MessageProcessor.Infrastructure;
using Messages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SigningService.Api.Client;

namespace MessageProcessor.Handlers;

public interface ISigningTriggeredHandler
{
    Task StartProcessingAsync(CancellationToken token);
    Task StopProcessingAsync();
}

public class SigningTriggeredHandler : ISigningTriggeredHandler
{
    private readonly ISigningClient _signingClient;
    private readonly IDocumentsClient _documentClient;
    private readonly AppSettings _appSettings;
    private readonly ServiceBusClient _serviceBusClient;

    readonly string QueueName = SigningTriggered.QueueName;

    public SigningTriggeredHandler(
        IServiceBusClientFactory serviceBusClientFactory, 
        ISigningClient signingClient,
        IDocumentsClient documentClient,
        IOptions<AppSettings> appSettings)
    {
        _signingClient = signingClient;
        _documentClient = documentClient;
        _appSettings = appSettings.Value;
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
        var payload = JsonConvert.DeserializeObject<SigningTriggered>(messageJson);
        var signingDataTasks = new List<Task<ICollection<SigningOutput>>>();

        int numSigningDataBatches = payload!.Documents.Count / _appSettings.SigningBatchSize;
        if (payload!.Documents.Count % _appSettings.SigningBatchSize != 0)
        {
            numSigningDataBatches++;
        }

        for (var i = 0; i < numSigningDataBatches; i++)
        {
            var batch = payload.Documents.GetRange(i, Math.Min(_appSettings.SigningBatchSize, payload.Documents.Count));
            var signedDataTask =  _signingClient.SignAsync(new SigningInput
            {
                KeyId = new Guid(payload.KeyId),
                Data = batch.Select(d => new DataItem
                {
                    Content = d.Content,
                    Id = d.DocumentId
                }).ToList()
            });
            signingDataTasks.Add(signedDataTask);
        }

        var singedDataCollection = await Task.WhenAll(signingDataTasks);

        var signedDataList = singedDataCollection.SelectMany(c => c).ToList();

        var storeSignedDataTasks = new List<Task>();
        int numStoringDataBatches = signedDataList.Count / _appSettings.CollectionServiceBatchSize;
        if (signedDataList.Count % _appSettings.CollectionServiceBatchSize != 0)
        {
            numStoringDataBatches++;
        }
        for (int i = 0; i < numStoringDataBatches; i++)
        {
            var batch = signedDataList.GetRange(i, Math.Min(_appSettings.CollectionServiceBatchSize, payload.Documents.Count));


            var signedDataTask = _documentClient.CreateSignedAsync(batch.Select(b => new AddSignedDocumentInput
            {
                Content = b.SignedData,
                DocumentId = b.Id
            }));
            storeSignedDataTasks.Add(signedDataTask);
        }

        await Task.WhenAll(storeSignedDataTasks);

        var signingCompletedMessageSender = _serviceBusClient.CreateSender(SigningCompleted.QueueName);
        var signingCompletedMessage = new SigningCompleted
        {
            KeyId = payload.KeyId
        };

        var messageBody = JsonConvert.SerializeObject(signingCompletedMessage);

        await signingCompletedMessageSender.SendMessageAsync(
            new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody)));

        await args.CompleteMessageAsync(message);
    }

    Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception}");
        return Task.CompletedTask;
    }
}