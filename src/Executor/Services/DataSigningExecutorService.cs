using System.Text;
using Azure.Messaging.ServiceBus;
using CollectionService.Api.Client;
using Executor.Infrastructure;
using Messages;
using Newtonsoft.Json;

namespace Executor.Services;

public class DataSigningExecutorService: IDataSigningExecutorService
{
    private readonly IDocumentsClient _documentClient;
    private readonly ServiceBusClient _serviceBusClient;

    public DataSigningExecutorService(
        IDocumentsClient documentClient,
        IServiceBusClientFactory serviceBusClientFactory)
    {
        _documentClient = documentClient;
        _serviceBusClient = serviceBusClientFactory.CreateClient();
    }
    public async Task ExecuteAsync(int batchSize)
    {
        var totalDataFetched = 0;
        var i = 1;
        while (true)
        {
            var collection = await _documentClient.GetAllUnsignedAsync(i, batchSize);
            i++;
            totalDataFetched += collection.Count;

            var message = new SigningTriggered
            {
                Documents = collection.Documents.Select(d => new Document
                {
                    DocumentId = d.Id,
                    Content = d.Content
                }).ToList(),
                TriggeredAtUtc = DateTimeOffset.Now,
                MessageId = new Guid()
            };


            var signingTriggeredMessageSender = _serviceBusClient.CreateSender(SigningTriggered.QueueName);
            
            var messageBody = JsonConvert.SerializeObject(message);

            await signingTriggeredMessageSender.SendMessageAsync(
                new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody)));

            if (collection.TotalItems == totalDataFetched)
            {
                break;
            }
        }
    }
}