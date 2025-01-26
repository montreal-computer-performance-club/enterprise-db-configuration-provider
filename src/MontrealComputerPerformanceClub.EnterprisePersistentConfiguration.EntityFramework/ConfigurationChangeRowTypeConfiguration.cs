using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public sealed class ConfigurationChangeRowTypeConfiguration : IEntityTypeConfiguration<ConfigurationChangeRow>
{
    public void Configure(EntityTypeBuilder<ConfigurationChangeRow> entity)
    {
        entity.ToTable("EpcConfigurationChanges");

        entity.HasKey((row) => row.Id);
        entity.Property((row) => row.Id).HasColumnName("id");

        entity.HasIndex((row) => row.Key).IsUnique();
        entity.Property((row) => row.Key).HasColumnName("key").IsRequired();

        entity.Property((row) => row.Value).HasColumnName("value");

        entity.HasIndex((row) => row.ChangeSetId);
        entity.Property((row) => row.ChangeSetId).HasColumnName("change_set_id").IsRequired();
    }
}
