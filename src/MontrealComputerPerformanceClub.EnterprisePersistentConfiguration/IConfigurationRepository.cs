namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

public readonly record struct ReadResult
{
    public sealed class Builder(ConfigurationTimeStamp latestTimeStamp)
    {
        public ImmutableArray<ConfigurationEntry>.Builder Entries { get; } =
            ImmutableArray.CreateBuilder<ConfigurationEntry>();

        public string this[string key]
        {
            set { Entries.Add(new ConfigurationEntry { Key = key, Value = value }); }
        }

        public ReadResult Build()
        {
            return new ReadResult
            {
                LatestTimeStamp = latestTimeStamp,
                Entries = Entries.DrainToImmutable(),
            };
        }
    }

    public ConfigurationTimeStamp LatestTimeStamp { get; init; }

    public ImmutableArray<ConfigurationEntry> Entries { get; init; }
}

public interface IConfigurationRepository : IDisposable, IAsyncDisposable
{
    Task<ReadResult> ReadSince(ConfigurationTimeStamp since, CancellationToken cancellationToken);

    Task RecordChangeSet(ConfigurationChangeSet changeSet, CancellationToken cancellationToken);
}
