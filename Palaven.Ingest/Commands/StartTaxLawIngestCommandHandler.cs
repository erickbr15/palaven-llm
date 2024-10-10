using Azure.Storage.Blobs.Models;
using Liara.Azure.BlobStorage;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Palaven.Model;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class StartTaxLawIngestCommandHandler : ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument>
{
    private readonly IBlobStorageService _storageService;
    private readonly IDocumentRepository<EtlTaskDocument> _repository;
    private readonly BlobStorageOptions _blobStorageOptions;

    public StartTaxLawIngestCommandHandler(IOptions<BlobStorageOptions> blobStorageOptions, IBlobStorageService blobStorageService, IDocumentRepository<EtlTaskDocument> repository)
    {
        _blobStorageOptions = blobStorageOptions?.Value ?? throw new ArgumentNullException(nameof(blobStorageOptions));
        _storageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return await Task.FromResult(Result<EtlTaskDocument>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) }));
        }

        var documentId = Guid.NewGuid();
        var traceId = Guid.NewGuid();
        var fileName = $"{documentId}{command.FileExtension}";

        var lawToIngestDocument = new StartTaxLawIngestTaskDocument
        {
            Id = documentId.ToString(),
            TraceId = traceId,
            TenantId = command.UserId,
            UserId = command.UserId,
            FileName = fileName,
            UntrustedFileName = command.UntrustedFileName,
            AcronymLaw = command.AcronymLaw,
            NameLaw = command.NameLaw,
            YearLaw = command.YearLaw,
            DocumentSchema = nameof(StartTaxLawIngestTaskDocument),
            IsTaskCompleted = false
        };        
        
        var result = await _repository.CreateAsync(lawToIngestDocument, new PartitionKey(lawToIngestDocument.UserId.ToString()), itemRequestOptions: null, cancellationToken);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Unable to create {nameof(StartTaxLawIngestTaskDocument)} document. Status code: {result.StatusCode}");
        }

        var uploadBlobModel = new AppendBlobModel
        {
            BlobContainerName = _blobStorageOptions.Containers[BlobStorageContainers.EtlInbox],
            BlobContent = command.FileContent,            
            BlobName = fileName,
            CreateOptions = new AppendBlobCreateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "TraceId", traceId.ToString() },
                    { "EtlTaskDocumentId", documentId.ToString() }
                }
            }
        };

        await _storageService.AppendAsync(uploadBlobModel, cancellationToken);

        return new Result<EtlTaskDocument> { Value = lawToIngestDocument };
    }    
}