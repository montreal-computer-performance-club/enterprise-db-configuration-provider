using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TUnit.Assertions.AssertConditions.Throws;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.Tests;

public sealed class ConfigurationRefreshServiceTests
{
    private readonly TimeSpan _refreshPeriod = TimeSpan.FromMinutes(5);

    private readonly FakeTimeProvider _clock = new();

    private readonly IConfigurationRepository _repository = A.Fake<IConfigurationRepository>();

    private readonly IRefreshableConfigurationProvider _provider =
        A.Fake<IRefreshableConfigurationProvider>();

    private readonly IServiceProvider _serviceProvider;

    public ConfigurationRefreshServiceTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<TimeProvider>(_clock)
            .AddSingleton(_repository)
            .AddSingleton(_provider)
            .BuildServiceProvider();
    }

    [Test]
    public async Task StartAsync_RefreshesCacheBeforeCompleting(CancellationToken cancellationToken)
    {
        var subject = new ConfigurationRefreshService(
            _refreshPeriod,
            _serviceProvider,
            NullLogger<ConfigurationRefreshService>.Instance
        );

        await subject.StartAsync(cancellationToken);

        A.CallTo(() => _provider.Refresh(_repository, A<CancellationToken>.Ignored))
            .MustHaveHappened(1, Times.Exactly);
    }

    [Test]
    public async Task StartAsync_RethrowsException(CancellationToken cancellationToken)
    {
        var subject = new ConfigurationRefreshService(
            _refreshPeriod,
            _serviceProvider,
            NullLogger<ConfigurationRefreshService>.Instance
        );

        A.CallTo(() => _provider.Refresh(_repository, A<CancellationToken>.Ignored))
            .Throws<IOException>();

        await Assert.That(subject.StartAsync(cancellationToken)).Throws<IOException>();
    }
}
