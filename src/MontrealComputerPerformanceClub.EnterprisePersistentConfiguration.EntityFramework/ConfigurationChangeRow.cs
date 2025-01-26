namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public sealed record class ConfigurationChangeRow
{
    public long Id { get; set; }

    public required string Key { get; set; }

    public string? Value { get; set; }

    public long ChangeSetId { get; set; }
}
