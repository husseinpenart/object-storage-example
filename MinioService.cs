using Amazon.S3;
using Amazon.S3.Transfer;

public class MinioService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public MinioService(IConfiguration config)
    {
        var cfg = config.GetSection("MinIO");
        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"http://{cfg["Endpoint"]}",
            ForcePathStyle = true
        };
        _s3 = new AmazonS3Client(cfg["AccessKey"], cfg["SecretKey"], s3Config);
        _bucket = cfg["BucketName"]!;
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        var key = $"{Guid.NewGuid()}-{file.FileName}";
        using var stream = file.OpenReadStream();
        var request = new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType
        };
        await _s3.PutObjectAsync(request);
        return key;
    }

    public async Task<Stream> DownloadAsync(string key)
    {
        var response = await _s3.GetObjectAsync(_bucket, key);
        return response.ResponseStream;
    }
}
