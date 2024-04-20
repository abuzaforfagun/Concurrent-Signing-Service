using DataSeeder.Config;
using DataSeeder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var host = CreateHostBuilder(args).Build();

var publicDataSeeder = host.Services.GetRequiredService<IPublicDataSeeder>();

if (!await publicDataSeeder.HasData())
{
    await publicDataSeeder.SeedDataAsync(100000);
}

var keyStoreDataSeeder = host.Services.GetRequiredService<IKeyStoreDataSeeder>();

if (!await keyStoreDataSeeder.HasData())
{
    await keyStoreDataSeeder.SeedDataAsync(100);
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

            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));

            services.AddTransient<IPublicDataSeeder, PublicDataSeeder>();
            services.AddTransient<IKeyStoreDataSeeder, KeyStoreDataSeeder>();
        });

    return builder;
}