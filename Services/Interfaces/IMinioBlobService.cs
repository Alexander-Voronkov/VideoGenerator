namespace VideoGenerator.Services.Interfaces;

public interface IMinioBlobService
{
	Task UploadAsync(string objectName, Stream data, string contentType);
	Task<Stream?> DownloadAsync(string objectName);
	Task<bool> ExistsAsync(string objectName);
	Task DeleteAsync(string objectName);
}
