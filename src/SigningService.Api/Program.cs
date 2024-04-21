using ConcurrentSigning.Cryptography;
using KeyManagement.Api.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection("Encryption"));

builder.Services.AddHttpClient<IKeysClient, KeysClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("KeysApi:BaseAddress"));
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(30));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
