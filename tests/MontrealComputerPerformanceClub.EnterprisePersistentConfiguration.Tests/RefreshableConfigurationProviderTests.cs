namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration.Tests;

public sealed class RefreshableConfigurationProviderTests
{
    [Test]
    public async Task GetChildKeys_NewInstance_Empty()
    {
        var subject = new RefreshableConfigurationProvider();
        await Assert.That(subject.GetChildKeys([], null)).IsEmpty();
    }

    [Test]
    public async Task Refresh_NewInstance_SinceDefault(CancellationToken cancellationToken)
    {
        var repository = A.Fake<IConfigurationRepository>();
        A.CallTo(
                () =>
                    repository.ReadSince(
                        A<ConfigurationTimeStamp>.Ignored,
                        A<CancellationToken>.Ignored
                    )
            )
            .Returns(
                new ReadResult { LatestTimeStamp = new ConfigurationTimeStamp(123), Entries = [] }
            );

        var subject = new RefreshableConfigurationProvider();
        await subject.Refresh(repository, cancellationToken);

        A.CallTo(
                () =>
                    repository.ReadSince(
                        new ConfigurationTimeStamp(0),
                        A<CancellationToken>.Ignored
                    )
            )
            .MustHaveHappened();
    }

    [Test]
    public async Task Refresh_StaleInstance_PreviousDefault(CancellationToken cancellationToken)
    {
        var repository = A.Fake<IConfigurationRepository>();
        A.CallTo(
                () =>
                    repository.ReadSince(
                        A<ConfigurationTimeStamp>.Ignored,
                        A<CancellationToken>.Ignored
                    )
            )
            .ReturnsNextFromSequence(
                new ReadResult { LatestTimeStamp = new ConfigurationTimeStamp(123), Entries = [] },
                new ReadResult { LatestTimeStamp = new ConfigurationTimeStamp(234), Entries = [] }
            );

        var subject = new RefreshableConfigurationProvider();
        await subject.Refresh(repository, cancellationToken);
        await subject.Refresh(repository, cancellationToken);

        A.CallTo(
                () =>
                    repository.ReadSince(
                        A<ConfigurationTimeStamp>.Ignored,
                        A<CancellationToken>.Ignored
                    )
            )
            .MustHaveHappened()
            .Then(
                A.CallTo(
                        () =>
                            repository.ReadSince(
                                new ConfigurationTimeStamp(123),
                                A<CancellationToken>.Ignored
                            )
                    )
                    .MustHaveHappened()
            );
    }

    [Test]
    public async Task Refresh_NewInstance_AddEntry(CancellationToken cancellationToken)
    {
        var repository = A.Fake<IConfigurationRepository>();
        A.CallTo(
                () =>
                    repository.ReadSince(
                        A<ConfigurationTimeStamp>.Ignored,
                        A<CancellationToken>.Ignored
                    )
            )
            .Returns(
                new ReadResult.Builder(new ConfigurationTimeStamp(123))
                {
                    ["Kestrel:EndpointDefaults:Protocols"] = "Http1AndHttp2",
                }.Build()
            );

        var subject = new RefreshableConfigurationProvider();
        await subject.Refresh(repository, cancellationToken);

        await Assert
            .That(subject.TryGet("Kestrel:EndpointDefaults:Protocols", out var value))
            .IsTrue();
        await Assert.That(value).IsEqualTo("Http1AndHttp2");
    }

    [Test]
    public async Task Refresh_StaleInstance_KeepsOtherExistingEntries(
        CancellationToken cancellationToken
    )
    {
        var repository = A.Fake<IConfigurationRepository>();
        A.CallTo(
                () =>
                    repository.ReadSince(
                        A<ConfigurationTimeStamp>.Ignored,
                        A<CancellationToken>.Ignored
                    )
            )
            .ReturnsNextFromSequence(
                new ReadResult.Builder(new ConfigurationTimeStamp(123))
                {
                    ["Kestrel:EndpointDefaults:Protocols"] = "Http1AndHttp2",
                }.Build(),
                new ReadResult.Builder(new ConfigurationTimeStamp(234))
                {
                    ["ConnectionStrings:Default"] =
                        @"Server=(localdb)\mssqllocaldb;Database=Test;Trusted_Connection=True",
                }.Build()
            );

        var subject = new RefreshableConfigurationProvider();
        await subject.Refresh(repository, cancellationToken);

        await subject.Refresh(repository, cancellationToken);
        await Assert
            .That(subject.TryGet("Kestrel:EndpointDefaults:Protocols", out var value))
            .IsTrue();
        await Assert.That(value).IsEqualTo("Http1AndHttp2");
    }

    [Test]
    public async Task Refresh_StaleInstance_ReplacesSameEntry(CancellationToken cancellationToken)
    {
        var repository = A.Fake<IConfigurationRepository>();
        A.CallTo(
                () =>
                    repository.ReadSince(
                        A<ConfigurationTimeStamp>.Ignored,
                        A<CancellationToken>.Ignored
                    )
            )
            .ReturnsNextFromSequence(
                new ReadResult.Builder(new ConfigurationTimeStamp(123))
                {
                    ["Kestrel:EndpointDefaults:Protocols"] = "Http1AndHttp2",
                }.Build(),
                new ReadResult.Builder(new ConfigurationTimeStamp(234))
                {
                    ["Kestrel:EndpointDefaults:Protocols"] = "Http1",
                }.Build()
            );

        var subject = new RefreshableConfigurationProvider();
        await subject.Refresh(repository, cancellationToken);

        await subject.Refresh(repository, cancellationToken);
        await Assert
            .That(subject.TryGet("Kestrel:EndpointDefaults:Protocols", out var value))
            .IsTrue();
        await Assert.That(value).IsEqualTo("Http1");
    }
}
