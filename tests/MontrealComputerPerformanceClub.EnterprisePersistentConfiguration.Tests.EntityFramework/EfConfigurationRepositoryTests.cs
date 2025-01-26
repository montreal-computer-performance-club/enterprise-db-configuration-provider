using Microsoft.EntityFrameworkCore;
using MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.Tests.EntityFramework;

[NotInParallel(nameof(TestDbContext))]
[ClassDataSource<TestDbContext>(Shared = SharedType.PerTestSession)]
public class EfConfigurationRepositoryTests(TestDbContext dbContext)
{
    private readonly EfConfigurationRepository<TestDbContext> _subject = new(dbContext);

    [Test]
    public async Task ReadSince_Default_Empty(CancellationToken cancellationToken)
    {
        var result = await _subject.ReadSince(default, cancellationToken);
        await Assert.That(result.Entries).IsEquivalentTo(ImmutableArray<ConfigurationEntry>.Empty);
    }

    [Test]
    [HasConfigurationChangeSet(Id = 11, UpdatedBy = "", UpdatedAt = "2025-02-15T11:11:11-4")]
    [HasConfigurationChange(Id = 22, Key = "A", Value = "1", ChangeSetId = 11)]
    [HasConfigurationEntry(Id = 33, Key = "A", Value = "1", ChangeId = 22)]
    public async Task ReadSince_Default_OneEntry(CancellationToken cancellationToken)
    {
        var result = await _subject.ReadSince(default, cancellationToken);
        await Assert
            .That(result.Entries)
            .IsEquivalentTo(
                ImmutableArray.Create(new ConfigurationEntry { Key = "A", Value = "1" })
            );
    }
}
