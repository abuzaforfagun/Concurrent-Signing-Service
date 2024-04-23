using Azure.Messaging.ServiceBus;

namespace Executor.Infrastructure;

public class ServiceBusClientFactory : IServiceBusClientFactory
{
    private readonly string _connectionString;

    public ServiceBusClientFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public ServiceBusClient CreateClient()
    {
        return new ServiceBusClient(_connectionString);
    }
}