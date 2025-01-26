namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public sealed record class ConfigurationEntryRow
{
    public long Id { get; set; }

    public required string Key { get; set; }

    public string? Value { get; set; }

    public long ChangeId { get; set; }

    public required ConfigurationChangeRow Change { get; set; }
}
