using Azure.Storage.Blobs;

namespace Dizimo.Services;

public class BackupService
{
    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly string _dbFilePath;

    public BackupService(string connectionString, string containerName, string dbFilePath)
    {
        _connectionString = connectionString;
        _containerName = containerName;
        _dbFilePath = dbFilePath;
    }

    public async Task BackupToCloudAsync()
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(Path.GetFileName(_dbFilePath));
        using var fileStream = File.OpenRead(_dbFilePath);
        await blobClient.UploadAsync(fileStream, overwrite: true);
    }

    public async Task RestoreFromCloudAsync()
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(Path.GetFileName(_dbFilePath));
        using var fileStream = File.OpenWrite(_dbFilePath);
        await blobClient.DownloadToAsync(fileStream);
    }
}
