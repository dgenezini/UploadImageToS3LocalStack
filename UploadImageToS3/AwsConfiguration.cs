namespace UploadImageToS3;

public class AwsConfiguration
{
    public string ServiceURL { get; set; }
    public string Region { get; set; }
    public bool ForcePathStyle { get; set; }
    public string AwsAccessKey { get; set; }
    public string AwsSecretKey { get; set; }
}