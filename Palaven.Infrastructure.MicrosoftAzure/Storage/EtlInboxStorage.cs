using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Liara.Integrations.Azure;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Palaven.Infrastructure.Abstractions.Storage;
using Palaven.Infrastructure.Model.Storage;

namespace Palaven.Infrastructure.MicrosoftAzure.Storage;

public class EtlInboxStorage : IEtlInboxStorage
{
    private readonly BlobContainerClient _etlInboxContainerClient;

    public EtlInboxStorage(IOptions<StorageAccountOptions> storageOptionsService, IAzureClientFactory<BlobServiceClient> blobServiceClientFactory)
    {        
        var blobServiceClient = blobServiceClientFactory.CreateClient("AzureStorageLawDocs") ?? throw new InvalidOperationException("Unable to create BlobServiceClient");
        var storageOptions = storageOptionsService.Value ?? throw new ArgumentNullException(nameof(storageOptionsService));

        _etlInboxContainerClient = blobServiceClient.GetBlobContainerClient(storageOptions.BlobContainers[BlobStorageContainers.EtlInbox]);
    }


    public async Task AppendAsync(string blobName, byte[] content, IDictionary<string, string> metadata, CancellationToken cancellationToken)
    {        
        var client = _etlInboxContainerClient.GetBlobClient(blobName);

        await client.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        await client.UploadAsync(new BinaryData(content), overwrite: true, cancellationToken: cancellationToken);
        await client.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
    }

    public async Task<BlobContent> DownloadAsync(string blobName, CancellationToken cancellationToken)
    {
        var client = _etlInboxContainerClient.GetBlobClient(blobName);

        var azureResponse = await client.DownloadContentAsync(cancellationToken);
        
        var blob = new BlobContent
        {
            BinaryData = azureResponse.Value.Content,            
            Metadata = azureResponse.Value.Details.Metadata.ToDictionary(x => x.Key, x => x.Value)            
        };

        return blob;
    }
}
