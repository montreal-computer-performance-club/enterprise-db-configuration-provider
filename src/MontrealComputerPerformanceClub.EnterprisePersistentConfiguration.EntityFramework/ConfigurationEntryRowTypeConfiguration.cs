using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public sealed class ConfigurationEntryRowTypeConfiguration
    : IEntityTypeConfiguration<ConfigurationEntryRow>
{
    public void Configure(EntityTypeBuilder<ConfigurationEntryRow> ty)
    {
        ty.ToTable("EpcConfigurationEntries");

        ty.HasKey((row) => row.Id);
        ty.Property((row) => row.Id).HasColumnName("id");

        ty.HasIndex((row) => row.Key);
        ty.Property((row) => row.Key).HasColumnName("key").IsRequired();

        ty.Property((row) => row.Value).HasColumnName("value");

        ty.HasIndex((row) => row.ChangeId).IsUnique();
        ty.Property((row) => row.ChangeId).HasColumnName("change_id").IsRequired();
        ty.HasOne((row) => row.Change)
            .WithOne()
            .HasForeignKey<ConfigurationEntryRow>((row) => row.ChangeId);
    }
}
