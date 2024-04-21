using MessageProcessor.Handlers;
using Microsoft.Extensions.Hosting;

namespace MessageProcessor.HostedService;

public class SigningTriggeredHandlerHostedService : IHostedService
{
    private readonly ISigningTriggeredHandler _signingTriggeredHandler;

    public SigningTriggeredHandlerHostedService(ISigningTriggeredHandler signingTriggeredHandler)
    {
        _signingTriggeredHandler = signingTriggeredHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _signingTriggeredHandler.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _signingTriggeredHandler.StopProcessingAsync();
    }
}