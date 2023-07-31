using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace UploadImageToS3.IntegrationTests.TestFixtures;

public class LocalStackTestcontainerFixture : IAsyncLifetime
{
    public const int LocalStackPort = 4566;
    public const string LocalStackImage = "localstack/localstack:1.3.1";

    public IContainer LocalStackTestcontainer { get; private set; } = default!;
    
    public async Task InitializeAsync()
    {
        LocalStackTestcontainer = new ContainerBuilder()
            .WithImage(LocalStackImage)
            .WithExposedPort(LocalStackPort)
            .WithPortBinding(LocalStackPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request
                    .ForPath("/_localstack/health")
                    .ForPort(LocalStackPort)))
            .Build();

        await LocalStackTestcontainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await LocalStackTestcontainer.DisposeAsync();
    }
}