using Microsoft.Extensions.Configuration;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

public sealed class RefreshableConfigurationProvider
    : ConfigurationProvider,
        IRefreshableConfigurationProvider
{
    private ConfigurationTimeStamp _freshestRowUpdatedAt = ConfigurationTimeStamp.Zero;

    public async Task Refresh(
        IConfigurationRepository repository,
        CancellationToken cancellationToken
    )
    {
        var data = new Dictionary<string, string?>(Data);

        var result = await repository.ReadSince(_freshestRowUpdatedAt, cancellationToken);
        foreach (var entry in result.Entries)
        {
            data[entry.Key] = entry.Value;
        }
        _freshestRowUpdatedAt = result.LatestTimeStamp;

        Data = data;
        // Reload is implemented with a CancellationTokenSource, which invokes its thunks either on
        // the current thread, or through a Send to the captured synchronization context, which (I
        // think?) implies a memory barrier. I need to look more into this, but in the meantime it
        // will probably work fine on x64 haha.
        OnReload();
    }
}
