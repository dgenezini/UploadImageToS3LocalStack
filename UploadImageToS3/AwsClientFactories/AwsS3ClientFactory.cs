using Amazon;
using Amazon.S3;

namespace UploadImageToS3.AwsClientFactories;

public static class AwsS3ClientFactory
{
    public static AmazonS3Client CreateAwsS3Client(string serviceUrl,
        string awsRegion, bool forcePathStyle,
        string awsAccessKey, string awsSecretKey)
    {
        var awsS3Config = GetAwsS3Config(serviceUrl, awsRegion, forcePathStyle);

        var s3Client = new AmazonS3Client(
            awsAccessKey, awsSecretKey,
            awsS3Config);

        return s3Client;
    }

    private static AmazonS3Config GetAwsS3Config(
        string localstackUrl, string awsRegion, bool forcePathStyle)
    {
        return new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(awsRegion),
            ServiceURL = localstackUrl,
            ForcePathStyle = forcePathStyle
        };
    }
}