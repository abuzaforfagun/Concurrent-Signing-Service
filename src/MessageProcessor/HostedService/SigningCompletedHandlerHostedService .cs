using MessageProcessor.Handlers;
using Microsoft.Extensions.Hosting;

namespace MessageProcessor.HostedService;

public class SigningCompletedHandlerHostedService : IHostedService
{
    private readonly ISigningCompletedHandler _signingCompletedHandler;

    public SigningCompletedHandlerHostedService(ISigningCompletedHandler signingCompletedHandler)
    {
        _signingCompletedHandler = signingCompletedHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _signingCompletedHandler.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _signingCompletedHandler.StopProcessingAsync();
    }
}