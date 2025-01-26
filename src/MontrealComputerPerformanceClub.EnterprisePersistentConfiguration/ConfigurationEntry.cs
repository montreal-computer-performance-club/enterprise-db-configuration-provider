namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

public readonly record struct ConfigurationEntry
{
    public string Key { get; init; }

    public string? Value { get; init; }
}
