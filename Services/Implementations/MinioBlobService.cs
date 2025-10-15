using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using VideoGenerator.Configs;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class MinioBlobService : IMinioBlobService
{
    private readonly IMinioClient _minio;

    public MinioBlobService(IOptions<MinioBlobConfig> settings)
    {
        _minio = new MinioClient()
            .WithEndpoint(settings.Value.Host)
            .WithCredentials(settings.Value.AccessKey, settings.Value.SecretKey)
            .WithSSL(false)
            .Build();
    }

    public async Task UploadAsync(
        string bucketName, 
        string objectName,
        Stream data,
        string contentType,
        CancellationToken token = default)
    {
        bool found = await _minio
            .BucketExistsAsync(new BucketExistsArgs()
            .WithBucket(bucketName), token);

        if (!found)
        {
            await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), token);
        }

        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType), token);
    }

    public async Task<MemoryStream?> DownloadAsync(string bucketName, string objectName, CancellationToken token = default)
    {
        var ms = new MemoryStream();
        try
        {
            await _minio.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms)), token);

            ms.Position = 0;
            return ms;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string bucketName, string objectName, CancellationToken token = default)
    {
        try
        {
            var exists = await _minio.StatObjectAsync(new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName), token);
            return exists is not null;
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteAsync(string bucketName, string objectName, CancellationToken token = default)
    {
        await _minio.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName), token);
    }

    public List<string> List(string bucketName, CancellationToken token = default)
    {
        var objectNames = new List<string>();

        var items = _minio.ListObjectsEnumAsync(
            new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(true),
            token);

        return items.ToBlockingEnumerable(token).Select(item => item.Key).ToList();
    }

    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiryInSeconds = 3600, CancellationToken token = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds);

        string url = await _minio.PresignedGetObjectAsync(args);
        return url;
    }
}
