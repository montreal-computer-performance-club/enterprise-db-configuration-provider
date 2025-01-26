using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.EntityFramework;

public static class HostApplicationBuilderPersistentConfigurationExtensions
{
    private static bool IsAdded;

    public static IHostApplicationBuilder AddDbConfigurationContext<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors
                | DynamicallyAccessedMemberTypes.NonPublicConstructors
                | DynamicallyAccessedMemberTypes.PublicProperties
        )]
            TDbContext
    >(this IHostApplicationBuilder builder, Action<DbContextOptionsBuilder> configureDbContext)
        where TDbContext : DbContext, IDbConfigurationContext
    {
        if (IsAdded)
        {
            throw new InvalidOperationException(
                "This library only supports sourcing configuration from a single database context."
            );
        }
        IsAdded = true;

        builder.Services.AddDbContext<TDbContext>(configureDbContext);

        var provider = new RefreshableConfigurationProvider();
        builder.Configuration.Add(new DbConfigurationSource(provider));
        builder.Services.AddSingleton(provider);

        builder.Services.AddScoped<
            IConfigurationRepository,
            EfConfigurationRepository<TDbContext>
        >();
        builder.Services.AddHostedService<ConfigurationRefreshService>();

        return builder;
    }
}
