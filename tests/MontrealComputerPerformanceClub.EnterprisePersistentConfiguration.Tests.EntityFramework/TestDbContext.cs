using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.Tests.EntityFramework;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HasConfigurationChangeSetAttribute : Attribute
{
    public required long Id { get; set; }

    public required string UpdatedBy { get; set; }

    [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
    public required string UpdatedAt { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class HasConfigurationChangeAttribute : Attribute
{
    public required long Id { get; set; }

    public required string Key { get; set; }

    [StringSyntax(StringSyntaxAttribute.Json)]
    public required string Value { get; set; }

    public required long ChangeSetId { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class HasConfigurationEntryAttribute : Attribute
{
    public required long Id { get; set; }

    public required string Key { get; set; }

    [StringSyntax(StringSyntaxAttribute.Json)]
    public required string Value { get; set; }

    public required long ChangeId { get; set; }
}

public class TestDbContext
    : DbContext,
        IDbConfigurationContext,
        ITestStartEventReceiver,
        ITestEndEventReceiver
{
    public DbSet<ConfigurationEntryRow> Entries { get; set; }

    public DbSet<ConfigurationChangeRow> Changes { get; set; }

    public DbSet<ConfigurationChangeSetRow> ChangeSets { get; set; }

    private string? _connectionString;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.AddPersistentConfigurationEntities();
    }

    public async ValueTask OnTestStart(BeforeTestContext beforeTestContext)
    {
        if (_connectionString is null)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<TestDbContext>()
                .Build();
            var connectionString = configuration.GetConnectionString("Test");
            if (connectionString is null)
            {
                throw new InvalidOperationException();
            }

            if (Interlocked.Exchange(ref _connectionString, connectionString) is null)
            {
                await Task.Run(() =>
                {
                    Database.EnsureDeleted();
                    Database.EnsureCreated();
                });
            }
        }

        using var operation = new SqlBulkCopy(_connectionString);
        var attributes = beforeTestContext.TestDetails.Attributes;
        var cancellationToken = beforeTestContext.TestContext.CancellationToken;

        operation.DestinationTableName = "EpcConfigurationChangeSets";
        await operation.WriteToServerAsync(
            PrepareChangeSets(attributes.OfType<HasConfigurationChangeSetAttribute>()),
            cancellationToken
        );

        operation.DestinationTableName = "EpcConfigurationChanges";
        await operation.WriteToServerAsync(
            PrepareChanges(attributes.OfType<HasConfigurationChangeAttribute>()),
            cancellationToken
        );

        operation.DestinationTableName = "EpcConfigurationEntries";
        await operation.WriteToServerAsync(
            PrepareEntries(attributes.OfType<HasConfigurationEntryAttribute>()),
            cancellationToken
        );
    }

    private static DataTable PrepareChangeSets(
        IEnumerable<HasConfigurationChangeSetAttribute> changeSets
    )
    {
        var table = new DataTable
        {
            Columns =
            {
                { "id", typeof(long) },
                { "updated_by", typeof(string) },
                { "updated_at", typeof(string) },
            },
        };
        foreach (var changeSet in changeSets)
        {
            table.Rows.Add(changeSet.Id, changeSet.UpdatedBy, changeSet.UpdatedAt);
        }
        return table;
    }

    private static DataTable PrepareChanges(IEnumerable<HasConfigurationChangeAttribute> changes)
    {
        var table = new DataTable
        {
            Columns =
            {
                { "id", typeof(long) },
                { "key", typeof(string) },
                { "value", typeof(string) },
                { "change_set_id", typeof(long) },
            },
        };
        foreach (var change in changes)
        {
            table.Rows.Add(change.Id, change.Key, change.Value, change.ChangeSetId);
        }
        return table;
    }

    private static DataTable PrepareEntries(IEnumerable<HasConfigurationEntryAttribute> changes)
    {
        var table = new DataTable
        {
            Columns =
            {
                { "id", typeof(long) },
                { "key", typeof(string) },
                { "value", typeof(string) },
                { "change_id", typeof(long) },
            },
        };
        foreach (var change in changes)
        {
            table.Rows.Add(change.Id, change.Key, change.Value, change.ChangeId);
        }
        return table;
    }

    public async ValueTask OnTestEnd(TestContext testContext)
    {
        await Entries.ExecuteDeleteAsync(testContext.CancellationToken);
        await Changes.ExecuteDeleteAsync(testContext.CancellationToken);
        await ChangeSets.ExecuteDeleteAsync(testContext.CancellationToken);
    }
}
