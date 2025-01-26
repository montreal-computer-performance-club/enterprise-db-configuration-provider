using Microsoft.Extensions.Configuration;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

public sealed class DbConfigurationSource(RefreshableConfigurationProvider provider) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return provider;
    }
}
