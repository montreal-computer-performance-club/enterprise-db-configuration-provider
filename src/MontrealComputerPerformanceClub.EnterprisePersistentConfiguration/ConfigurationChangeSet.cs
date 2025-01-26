namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

public sealed record class ConfigurationChangeSet
{
    public required string UpdatedBy { get; init; }

    public required ImmutableArray<ConfigurationEntry> Changes { get; init; }
}
