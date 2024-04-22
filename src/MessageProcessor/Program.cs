using CollectionService.Api.Client;
using MessageProcessor.Config;
using MessageProcessor.Handlers;
using MessageProcessor.HostedService;
using MessageProcessor.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SigningService.Api.Client;

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
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            services.AddSingleton<IServiceBusClientFactory>(new ServiceBusClientFactory(serviceBusConnectionString));
            services.AddSingleton<ISigningTriggeredHandler, SigningTriggeredHandler>();
            services.AddHttpClient<IDocumentsClient, DocumentsClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("CollectionsApi:BaseAddress"));
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(30));

            services.AddHttpClient<ISigningClient, SigningClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("SigningApi:BaseAddress"));
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(30));

            services.AddHostedService<SigningTriggeredHandlerHostedService>();
        });

    return builder;
}