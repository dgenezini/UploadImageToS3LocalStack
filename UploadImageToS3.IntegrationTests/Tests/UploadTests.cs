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
public class UploadTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IContainer _localStackTestcontainer;

    public UploadTests(CustomWebApplicationFactory factory,
        LocalStackTestcontainerFixture localStackTestcontainerFixture)
    {
        _factory = factory;
        _localStackTestcontainer = localStackTestcontainerFixture.LocalStackTestcontainer;
    }

    [Fact]
    public async Task UploadObject_Returns200()
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

        //Act
        using var multipartFormContent = new MultipartFormDataContent();

        var fileStreamContent = new StreamContent(File.OpenRead(filePath));
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

        multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName);

        var httpResponse = await HttpClient.PostAsync($"/upload", multipartFormContent);

        //Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var bucketFile = await s3Client.GetObjectAsync(configuration["BucketName"],
            fileName);

        bucketFile.HttpStatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadExistentObject_Returns200AndOverride()
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

        //Act
        using var multipartFormContent = new MultipartFormDataContent();

        var fileStreamContent = new StreamContent(File.OpenRead(filePath));
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

        multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName);

        var httpResponse1 = await HttpClient.PostAsync($"/upload", multipartFormContent);

        var httpResponse2 = await HttpClient.PostAsync($"/upload", multipartFormContent);

        //Assert
        httpResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        httpResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var bucketObjects = await s3Client.ListObjectsAsync(configuration["BucketName"]);

        bucketObjects.S3Objects.Count.Should().Be(1);

        var bucketFile = await s3Client.GetObjectAsync(configuration["BucketName"],
            fileName);

        bucketFile.HttpStatusCode.Should().Be(HttpStatusCode.OK);
    }
}