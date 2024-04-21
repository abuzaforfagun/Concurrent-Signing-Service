using MessageProcessor.Handlers;
using MessageProcessor.HostedService;
using MessageProcessor.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = CreateHostBuilder(args).Build();
await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args)
{
    var builder = Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(configHost =>
        {
            configHost.AddJsonFile("appsettings.json", true);
            configHost.AddEnvironmentVariables();
        })
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            var serviceBusConnectionString = configuration.GetValue<string>("ServiceBus");
            services.AddOptions();

            services.AddSingleton<IServiceBusClientFactory>(new ServiceBusClientFactory(serviceBusConnectionString));
            services.AddSingleton<ISigningTriggeredHandler, SigningTriggeredHandler>();

            services.AddHostedService<SigningTriggeredHandlerHostedService>();
        });

    return builder;
}