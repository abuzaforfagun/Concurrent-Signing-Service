using CollectionService.Api.Client;
using Executor.Config;
using Executor.Infrastructure;
using Executor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = CreateHostBuilder(args).Build();
var executorService = host.Services.GetRequiredService<IDataSigningExecutorService>();
var appSettings = host.Services.GetRequiredService<IOptions<AppSettings>>().Value;

await executorService.ExecuteAsync(appSettings.BatchSize);

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

            services.AddHttpClient<IDocumentsClient, DocumentsClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("CollectionsApi:BaseAddress"));
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(30));
            
            services.AddSingleton<IDataSigningExecutorService, DataSigningExecutorService>();
        });

    return builder;
}
