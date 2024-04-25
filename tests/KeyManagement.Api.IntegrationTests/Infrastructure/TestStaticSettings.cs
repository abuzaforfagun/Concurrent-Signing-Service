using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace KeyManagement.Api.IntegrationTests.Infrastructure;

public static class TestStaticSettings
{
    private static readonly Lazy<IConfiguration> _configuration = new(() => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables()
        .Build());

    public static IConfiguration Configuration => _configuration.Value;

    public static string KeyStoreDbConnectionString => _configuration.Value.GetValue<string>("Database:KeyStoreConnectionString");
    public static string PrivateKey => _configuration.Value.GetValue<string>("Encryption:PrivateKey");
}