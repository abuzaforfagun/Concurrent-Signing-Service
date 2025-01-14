using Asp.Versioning;
using ConcurrentSigning.Cryptography;
using KeyManagement.Api.Config;
using KeyManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services
    .AddApiVersioning(o =>
    {
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddSwaggerDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Title = "Key Management API";
    };
});
builder.Services.AddOptions();

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection("Encryption"));

builder.Services.AddSingleton<IKeyStorageService, KeyStorageService>();

var app = builder.Build();

app.UseRouting();
app.UseOpenApi();
app.UseSwaggerUi3();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.Run();

public partial class Program{}