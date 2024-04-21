using Azure.Messaging.ServiceBus;

namespace MessageProcessor.Infrastructure;

public interface IServiceBusClientFactory
{
    ServiceBusClient CreateClient();
}