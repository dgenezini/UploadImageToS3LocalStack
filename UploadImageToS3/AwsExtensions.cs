using Amazon;
using Amazon.S3;

namespace UploadImageToS3;

public static class AwsExtensions
{
    public static void AddAwsS3Service(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("AWS") is null)
        {
            builder.Services.AddAWSService<IAmazonS3>();
        }
        else
        {
            builder.Services.AddSingleton<IAmazonS3>(sc =>
            {
                var awsS3Config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]),
                    ServiceURL = builder.Configuration["AWS:ServiceURL"],
                    ForcePathStyle = bool.Parse(builder.Configuration["AWS:ForcePathStyle"]!)
                };

                return new AmazonS3Client(awsS3Config);
            });
        }
    }
}