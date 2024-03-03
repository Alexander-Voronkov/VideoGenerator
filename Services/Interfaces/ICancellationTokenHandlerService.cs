namespace VideoGenerator.Services.Interfaces;

public interface ICancellationTokenHandlerService
{
    CancellationToken Token { get; }
}