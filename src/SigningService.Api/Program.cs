using Asp.Versioning;
using ConcurrentSigning.Cryptography;
using KeyManagement.Api.Client;
using Microsoft.AspNetCore.Mvc;

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
        document.Info.Title = "Content Signing API";
    };
});
builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection("Encryption"));

builder.Services.AddHttpClient<IKeysClient, KeysClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("KeysApi:BaseAddress"));
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(30));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseOpenApi();
app.UseSwaggerUi3();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
