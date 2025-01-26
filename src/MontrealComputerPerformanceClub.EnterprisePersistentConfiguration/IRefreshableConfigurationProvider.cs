using Microsoft.Extensions.Configuration;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

public interface IRefreshableConfigurationProvider : IConfigurationProvider
{
    Task Refresh(IConfigurationRepository repository, CancellationToken cancellationToken);
}
