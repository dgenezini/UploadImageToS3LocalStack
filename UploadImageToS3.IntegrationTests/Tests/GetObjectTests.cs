using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using UploadImageToS3.AwsClientFactories;
using UploadImageToS3.IntegrationTests.TestFixtures;

namespace UploadImageToS3.IntegrationTests.Tests;

[Collection("LocalStackTestcontainer Collection")]
public class GetObjectTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IContainer _localStackTestcontainer;

    public GetObjectTests(CustomWebApplicationFactory factory,
        LocalStackTestcontainerFixture localStackTestcontainerFixture)
    {
        _factory = factory;
        _localStackTestcontainer = localStackTestcontainerFixture.LocalStackTestcontainer;
    }

    [Fact]
    public async Task GetExistingObject_Returns200()
    {
        //Arrange
        var localstackUrl = $"http://{_localStackTestcontainer.Hostname}:{_localStackTestcontainer.GetMappedPublicPort(4566)}";

        var HttpClient = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("AWS:ServiceURL", localstackUrl);
            })
            .CreateClient();

        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var awsConfiguration = configuration.GetSection("AWS").Get<AwsConfiguration>()!;

        var s3Client = AwsS3ClientFactory.CreateAwsS3Client(localstackUrl,
            awsConfiguration.Region, awsConfiguration.ForcePathStyle,
            awsConfiguration.AwsAccessKey, awsConfiguration.AwsSecretKey);

        await s3Client.PutBucketAsync(configuration["BucketName"]);

        const string fileName = "upload.jpg";

        var filePath = Path.Combine(Directory.GetCurrentDirectory(),
            "Assets", fileName);

        var putObjectRequest = new PutObjectRequest()
        {
            BucketName = configuration["BucketName"],
            Key = fileName,
            FilePath = filePath
        };

        putObjectRequest.Metadata.Add("Content-Type", "image/jpg");

        var putResult = await s3Client.PutObjectAsync(putObjectRequest);

        //Act
        var httpResponse = await HttpClient.GetAsync($"/object/{fileName}");

        //Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.Should().Be(MediaTypeHeaderValue.Parse("image/jpeg"));
    }

    [Fact]
    public async Task GetInexistingObject_Returns404()
    {
        //Arrange
        var localstackUrl = $"http://{_localStackTestcontainer.Hostname}:{_localStackTestcontainer.GetMappedPublicPort(4566)}";

        var HttpClient = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("AWS:ServiceURL", localstackUrl);
            })
            .CreateClient();

        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var awsConfiguration = configuration.GetSection("AWS").Get<AwsConfiguration>()!;

        var s3Client = AwsS3ClientFactory.CreateAwsS3Client(localstackUrl,
            awsConfiguration.Region, awsConfiguration.ForcePathStyle,
            awsConfiguration.AwsAccessKey, awsConfiguration.AwsSecretKey);

        await s3Client.PutBucketAsync(configuration["BucketName"]);

        const string fileName = "inexisting.jpg";

        //Act
        var httpResponse = await HttpClient.GetAsync($"/object/{fileName}");

        //Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}