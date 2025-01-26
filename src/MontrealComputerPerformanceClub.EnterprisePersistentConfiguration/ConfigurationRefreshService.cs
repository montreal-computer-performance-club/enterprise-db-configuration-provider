using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

public sealed partial class ConfigurationRefreshService(
    TimeSpan period,
    IServiceProvider serviceProvider,
    ILogger<ConfigurationRefreshService> logger
) : IHostedService, IAsyncDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    private PeriodicTimer? _timer;

    private Task? _task;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_cancellationTokenSource is not null || _timer is not null || _task is not null)
        {
            throw new InvalidOperationException();
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken
        );

        await Refresh(cancellationToken);

        _timer = new PeriodicTimer(period, serviceProvider.GetRequiredService<TimeProvider>());
        _task = ExecuteAsync(_timer, _cancellationTokenSource.Token);
    }

    private async Task Refresh(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var provider =
            scope.ServiceProvider.GetRequiredService<IRefreshableConfigurationProvider>();
        var repository = scope.ServiceProvider.GetRequiredService<IConfigurationRepository>();
        await provider.Refresh(repository, cancellationToken);
    }

    private async Task ExecuteAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    await Refresh(cancellationToken);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    LogRefreshException(e);
                }
            }
        }
        catch (OperationCanceledException)
        {
            LogServiceExitOnCancellation();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cancellationTokenSource is null || _timer is null || _task is null)
        {
            throw new InvalidOperationException();
        }

        _timer.Dispose();
        await _cancellationTokenSource.CancelAsync();
        await _task;
    }

    public ValueTask DisposeAsync()
    {
        _cancellationTokenSource?.Dispose();
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Configuration polling cancelled.")]
    private partial void LogServiceExitOnCancellation();

    [LoggerMessage(LogLevel.Error, "An exception occured while refreshing the configuration.")]
    private partial void LogRefreshException(Exception e);
}
