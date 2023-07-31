using Amazon.S3;
using UploadImageToS3.AwsClientFactories;

namespace UploadImageToS3;

public static class AwsExtensions
{
    public static void AddAwsServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAmazonS3>(sc =>
        {
            var configuration = sc.GetRequiredService<IConfiguration>();
            var awsConfiguration = configuration.GetSection("AWS").Get<AwsConfiguration>();

            if (awsConfiguration?.ServiceURL is null)
            {
                return new AmazonS3Client();
            }
            else
            {
                return AwsS3ClientFactory.CreateAwsS3Client(
                    awsConfiguration.ServiceURL,
                    awsConfiguration.Region, awsConfiguration.ForcePathStyle,
                    awsConfiguration.AwsAccessKey, awsConfiguration.AwsSecretKey);
            }
        });
    }
}