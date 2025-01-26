using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public sealed class ConfigurationChangeSetRowTypeConfiguration
    : IEntityTypeConfiguration<ConfigurationChangeSetRow>
{
    public void Configure(EntityTypeBuilder<ConfigurationChangeSetRow> entity)
    {
        entity.ToTable("EpcConfigurationChangeSets");

        entity.HasKey((row) => row.Id);
        entity.Property((row) => row.Id).HasColumnName("id");

        entity.Property((row) => row.UpdatedBy).HasColumnName("updated_by").IsRequired();
        entity
            .Property((row) => row.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .ValueGeneratedOnAdd();
        entity.HasIndex((row) => new { row.UpdatedBy, row.UpdatedAt });

        entity.HasMany((row) => row.Changes).WithOne().HasForeignKey((row) => row.ChangeSetId);
    }
}
