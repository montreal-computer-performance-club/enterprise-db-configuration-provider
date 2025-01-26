using Microsoft.EntityFrameworkCore;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public sealed class EfConfigurationRepository<TConfigurationDbContext>(
    TConfigurationDbContext dbContext
) : IConfigurationRepository
    where TConfigurationDbContext : DbContext, IDbConfigurationContext
{
    public async Task<ReadResult> ReadSince(
        ConfigurationTimeStamp since,
        CancellationToken cancellationToken
    )
    {
        var rows = dbContext.Entries.AsQueryable();
        if (since != default)
        {
            var sinceStamp = since.Stamp;
            rows = rows.Where((row) => row.ChangeId >= sinceStamp);
        }
        var rowList = await rows.Select(
                (row) =>
                    new
                    {
                        row.Key,
                        row.Value,
                        row.ChangeId,
                    }
            )
            .ToListAsync(cancellationToken);

        var entries = ImmutableArray.CreateBuilder<ConfigurationEntry>(rowList.Count);
        foreach (var row in rowList)
        {
            since = ConfigurationTimeStamp.Max(since, new ConfigurationTimeStamp(row.ChangeId));
            entries.Add(new ConfigurationEntry { Key = row.Key, Value = row.Value });
        }
        return new ReadResult { LatestTimeStamp = since, Entries = entries.MoveToImmutable() };
    }

    public async Task RecordChangeSet(
        ConfigurationChangeSet changeSet,
        CancellationToken cancellationToken
    )
    {
        var changeRows = new List<ConfigurationChangeRow>();
        foreach (var change in changeSet.Changes)
        {
            var changeRow = new ConfigurationChangeRow { Key = change.Key, Value = change.Value };
            dbContext.Changes.Add(changeRow);
            changeRows.Add(changeRow);
        }
        var changeRowsToProcess = changeRows.ToDictionary((row) => row.Key);

        var entryRows = await dbContext
            .Entries.Where((row) => changeRowsToProcess.Keys.Contains(row.Key))
            .ToListAsync(cancellationToken);
        foreach (var entryRow in entryRows)
        {
            if (changeRowsToProcess.Remove(entryRow.Key, out var changeRow))
            {
                entryRow.Value = changeRow.Value;
                entryRow.Change = changeRow;
            }
        }

        foreach (var changeRow in changeRowsToProcess.Values)
        {
            dbContext.Entries.Add(
                new ConfigurationEntryRow
                {
                    Key = changeRow.Key,
                    Value = changeRow.Value,
                    Change = changeRow,
                }
            );
        }

        dbContext.ChangeSets.Add(
            new ConfigurationChangeSetRow { UpdatedBy = changeSet.UpdatedBy, Changes = changeRows }
        );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return dbContext.DisposeAsync();
    }
}
