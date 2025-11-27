using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System.Text.Json;
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
            .WithSSL(settings.Value.Ssl)
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

    public async Task DownloadAsync(string bucketName, string objectName, Stream destination, CancellationToken token = default)
    {
        try
        {
            await _minio.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(destination)), token);
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
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

    public async Task<List<string>> ListAsync(string bucketName, CancellationToken token = default)
    {
        var objectNames = new List<string>();

        await foreach(var item in _minio.ListObjectsEnumAsync(
            new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(true),
            token))
        {
            objectNames.Add(item.Key);
		}

        return objectNames;
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

	public async Task MakeBucketPublicAsync(string bucketName, CancellationToken token = default)
	{
		bool exists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), token);
		if (!exists)
		{
			await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), token);
		}

		var policy = new
		{
			Version = "2012-10-17",
			Statement = new[]
			{
			new
			{
				Effect = "Allow",
				Principal = new { AWS = new[] { "*" } },
				Action = new[] { "s3:GetBucketLocation", "s3:ListBucket" },
				Resource = new[] { $"arn:aws:s3:::{bucketName}" }
			},
			new
			{
				Effect = "Allow",
				Principal = new { AWS = new[] { "*" } },
				Action = new[] { "s3:GetObject" },
				Resource = new[] { $"arn:aws:s3:::{bucketName}/*" }
			}
		}
		};

		string policyJson = JsonSerializer.Serialize(policy);

		await _minio.SetPolicyAsync(new SetPolicyArgs()
			.WithBucket(bucketName)
			.WithPolicy(policyJson), token);
	}
}
