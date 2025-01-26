namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public sealed record class ConfigurationChangeSetRow
{
    public long Id { get; set; }

    public required string UpdatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ConfigurationChangeRow> Changes { get; set; } = [];
}
