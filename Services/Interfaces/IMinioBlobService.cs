namespace VideoGenerator.Services.Interfaces;

public interface IMinioBlobService
{
	Task UploadAsync(string bucketName, string objectName, Stream data, string contentType, CancellationToken token);
	Task DownloadAsync(string bucketName, string objectName, Stream stream, CancellationToken token);
	Task<bool> ExistsAsync(string bucketName, string objectName, CancellationToken token);
	Task DeleteAsync(string bucketName, string objectName, CancellationToken token);
	Task<List<string>> ListAsync(string bucketName, CancellationToken token = default);
	Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiryInSeconds = 3600, CancellationToken token = default);
	Task MakeBucketPublicAsync(string bucketName, CancellationToken token = default);
}
