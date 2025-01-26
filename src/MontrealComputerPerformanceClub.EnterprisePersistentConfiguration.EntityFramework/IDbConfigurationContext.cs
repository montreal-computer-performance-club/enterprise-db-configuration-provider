using Microsoft.EntityFrameworkCore;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public interface IDbConfigurationContext
{
    DbSet<ConfigurationEntryRow> Entries { get; }

    DbSet<ConfigurationChangeRow> Changes { get; }

    DbSet<ConfigurationChangeSetRow> ChangeSets { get; }
}
