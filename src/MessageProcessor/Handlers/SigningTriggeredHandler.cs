using System.Text;
using Azure.Messaging.ServiceBus;
using CollectionService.Api.Client;
using KeyManagement.Api.Client;
using MessageProcessor.Config;
using MessageProcessor.Infrastructure;
using Messages;
using Microsoft.Extensions.Logging;
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
    private readonly IKeysClient _keysClient;
    private readonly ILogger<SigningTriggeredHandler> _logger;
    private readonly AppSettings _appSettings;
    private readonly ServiceBusClient _serviceBusClient;

    readonly string QueueName = SigningTriggered.QueueName;

    public SigningTriggeredHandler(
        IServiceBusClientFactory serviceBusClientFactory, 
        ISigningClient signingClient,
        IDocumentsClient documentClient,
        IKeysClient keysClient,
        ILogger<SigningTriggeredHandler> logger,
        IOptions<AppSettings> appSettings)
    {
        _signingClient = signingClient;
        _documentClient = documentClient;
        _keysClient = keysClient;
        _logger = logger;
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

        GetKeyOutput signingKey = null;
        try
        {
            signingKey = await _keysClient.PopAsync();
            _logger.LogInformation("Signing key ID: {0}", signingKey.Id);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError("Unable to find signing key, scheduling the message");
            var signingCompletedMessageSender = _serviceBusClient.CreateSender(SigningTriggered.QueueName);

            await signingCompletedMessageSender.ScheduleMessageAsync(
                new ServiceBusMessage(Encoding.UTF8.GetBytes(messageJson)), DateTimeOffset.UtcNow.AddMinutes(5));
            
            await args.CompleteMessageAsync(message);
        }

        try
        {
            var signingDataTasks = new List<Task<ICollection<SigningOutput>>>();

            int numSigningDataBatches = payload!.Documents.Count / _appSettings.SigningBatchSize;
            if (payload!.Documents.Count % _appSettings.SigningBatchSize != 0)
            {
                numSigningDataBatches++;
            }

            var traversedSigningIndex = 0;
            for (var i = 0; i < numSigningDataBatches; i++)
            {
                var batch = payload.Documents.GetRange(traversedSigningIndex,
                    Math.Min(_appSettings.SigningBatchSize, payload.Documents.Count));
                var signedDataTask = _signingClient.SignAsync(new SigningInput
                {
                    PrivateKey = signingKey.PrivateKey,
                    Data = batch.Select(d => new DataItem
                    {
                        Content = d.Content,
                        Id = d.DocumentId
                    }).ToList()
                });
                signingDataTasks.Add(signedDataTask);
                traversedSigningIndex += _appSettings.SigningBatchSize;
            }

            var singedDataCollection = await Task.WhenAll(signingDataTasks);

            var signedDataList = singedDataCollection.SelectMany(c => c).ToList();
            _logger.LogInformation("{0} data is signed by Signing Service", signedDataList.Count);

            var storeSignedDataTasks = new List<Task>();
            int numStoringDataBatches = signedDataList.Count / _appSettings.CollectionServiceBatchSize;
            if (signedDataList.Count % _appSettings.CollectionServiceBatchSize != 0)
            {
                numStoringDataBatches++;
            }

            var traversedStoringIndex = 0;
            for (int i = 0; i < numStoringDataBatches; i++)
            {
                var batch = signedDataList.GetRange(traversedStoringIndex,
                    Math.Min(_appSettings.CollectionServiceBatchSize, payload.Documents.Count));


                var signedDataTask = _documentClient.CreateSignedAsync(batch.Select(b => new AddSignedDocumentInput
                {
                    Content = b.SignedData,
                    DocumentId = b.Id
                }));
                storeSignedDataTasks.Add(signedDataTask);
                traversedStoringIndex += _appSettings.CollectionServiceBatchSize;
            }

            await Task.WhenAll(storeSignedDataTasks);

            var signingCompletedMessageSender = _serviceBusClient.CreateSender(SigningCompleted.QueueName);
            var signingCompletedMessage = new SigningCompleted
            {
                KeyId = signingKey.Id.ToString()
            };

            var messageBody = JsonConvert.SerializeObject(signingCompletedMessage);

            await signingCompletedMessageSender.SendMessageAsync(
                new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody)));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to complete the signing", ex);
            await _keysClient.ReleaseLockAsync(signingKey.Id);
            throw;
        }

        await args.CompleteMessageAsync(message);
        _logger.LogInformation("Signing completed");
    }

    Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError($"Error occurred: {args.Exception}");
        return Task.CompletedTask;
    }
}