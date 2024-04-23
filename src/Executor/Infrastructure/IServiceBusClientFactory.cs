using Azure.Messaging.ServiceBus;

namespace Executor.Infrastructure;

public interface IServiceBusClientFactory
{
    ServiceBusClient CreateClient();
}