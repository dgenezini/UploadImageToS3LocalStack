using Amazon.S3;
using Amazon.S3.Model;
using UploadImageToS3;

var builder = WebApplication.CreateBuilder(args);

builder.AddAwsServices();

var app = builder.Build();

app.MapPost("/upload", async (IAmazonS3 s3Client, IFormFile file) =>
{
    var bucketName = builder.Configuration["BucketName"]!;

    var bucketExists = await s3Client.DoesS3BucketExistAsync(bucketName);

    if (!bucketExists)
    {
        return Results.BadRequest($"Bucket {bucketName} does not exists.");
    }

    using var fileStream = file.OpenReadStream();

    var putObjectRequest = new PutObjectRequest()
    {
        BucketName = bucketName,
        Key = file.FileName,
        InputStream = fileStream
    };

    putObjectRequest.Metadata.Add("Content-Type", file.ContentType);

    var putResult = await s3Client.PutObjectAsync(putObjectRequest);

    return Results.Ok($"File {file.FileName} uploaded to S3 successfully!");
});

app.MapGet("/object/{key}", async (IAmazonS3 s3Client, string key) =>
{
    var bucketName = builder.Configuration["BucketName"]!;

    var bucketExists = await s3Client.DoesS3BucketExistAsync(bucketName);

    if (!bucketExists)
    {
        return Results.BadRequest($"Bucket {bucketName} does not exists.");
    }

    try
    {
        var getObjectResponse = await s3Client.GetObjectAsync(bucketName,
            key);

        return Results.File(getObjectResponse.ResponseStream,
            getObjectResponse.Headers.ContentType);
    }
    catch (AmazonS3Exception ex) when (ex.ErrorCode.Equals("NotFound", StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound();
    }
});

app.Run();