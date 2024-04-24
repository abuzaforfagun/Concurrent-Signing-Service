using DataSeeder.Config;
using DataSeeder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

//TODO: Move the wait to docker-compose
var isRunningFromDockerCompose = config.GetValue<bool>("IsRunningFromDockerCompose");
if (isRunningFromDockerCompose)
{
    await Task.Delay(180000);
}

var host = CreateHostBuilder(args).Build();

var publicDataSeeder = host.Services.GetRequiredService<IPublicDataSeeder>();
var appSettings = host.Services.GetRequiredService<IOptions<AppSettings>>().Value;
if (!await publicDataSeeder.HasData())
{
    await publicDataSeeder.SeedDataAsync(appSettings.NumberOfMockDocument);
}

var keyStoreDataSeeder = host.Services.GetRequiredService<IKeyStoreDataSeeder>();

if (!await keyStoreDataSeeder.HasData())
{
    await keyStoreDataSeeder.SeedDataAsync(appSettings.NumberOfMockKeys);
}

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

            services.AddOptions();
            services.AddLogging();

            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            services.AddTransient<IPublicDataSeeder, PublicDataSeeder>();
            services.AddTransient<IKeyStoreDataSeeder, KeyStoreDataSeeder>();
        });

    return builder;
}