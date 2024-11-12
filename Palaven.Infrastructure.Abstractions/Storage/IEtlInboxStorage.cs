using Palaven.Infrastructure.Model.Storage;

namespace Palaven.Infrastructure.Abstractions.Storage;

public interface IEtlInboxStorage
{
    Task AppendAsync(string blobName, byte[] content, IDictionary<string, string> metadata, CancellationToken cancellationToken);
    Task<BlobContent> DownloadAsync(string blobName, CancellationToken cancellationToken);
}
