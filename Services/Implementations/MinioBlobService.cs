using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using VideoGenerator.Configs;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;
public class MinioBlobService : IMinioBlobService
{
	private readonly IMinioClient _minio;
	private readonly string _bucketName;

	public MinioBlobService(IOptions<MinioBlobConfig> settings)
	{
		_bucketName = settings.Value.BucketName;

		_minio = new MinioClient()
			.WithEndpoint(settings.Value.Host)
			.WithCredentials(settings.Value.AccessKey, settings.Value.SecretKey)
			.WithSSL(false)
			.Build();
	}

	public async Task UploadAsync(string objectName, Stream data, string contentType)
	{
		bool found = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
		if (!found)
		{
			await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
		}

		await _minio.PutObjectAsync(new PutObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(objectName)
			.WithStreamData(data)
			.WithObjectSize(data.Length)
			.WithContentType(contentType));
	}

	public async Task<Stream?> DownloadAsync(string objectName)
	{
		MemoryStream ms = new MemoryStream();
		try
		{
			await _minio.GetObjectAsync(new GetObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(objectName)
				.WithCallbackStream(stream => stream.CopyTo(ms)));

			ms.Position = 0;
			return ms;
		}
		catch (Minio.Exceptions.ObjectNotFoundException)
		{
			return null;
		}
	}

	public async Task<bool> ExistsAsync(string objectName)
	{
		try
		{
			await _minio.StatObjectAsync(new StatObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(objectName));
			return true;
		}
		catch
		{
			return false;
		}
	}

	public async Task DeleteAsync(string objectName)
	{
		await _minio.RemoveObjectAsync(new RemoveObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(objectName));
	}
}
