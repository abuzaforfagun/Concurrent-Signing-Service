namespace Executor.Services;

public interface IDataSigningExecutorService
{
    Task ExecuteAsync(int batchSize);
}