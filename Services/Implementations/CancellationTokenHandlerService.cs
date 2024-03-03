using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class CancellationTokenHandlerService : ICancellationTokenHandlerService
{
    private readonly CancellationTokenSource _tokenSource = new();

    public CancellationToken Token => _tokenSource.Token;
}