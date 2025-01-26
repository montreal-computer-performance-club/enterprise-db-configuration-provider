using Microsoft.EntityFrameworkCore;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public static class ModelBuilderPersistentConfigurationExtensions
{
    public static ModelBuilder AddPersistentConfigurationEntities(this ModelBuilder model)
    {
        new ConfigurationEntryRowTypeConfiguration().Configure(model.Entity<ConfigurationEntryRow>());
        new ConfigurationChangeRowTypeConfiguration().Configure(model.Entity<ConfigurationChangeRow>());
        new ConfigurationChangeSetRowTypeConfiguration().Configure(model.Entity<ConfigurationChangeSetRow>());
        return model;
    }
}
