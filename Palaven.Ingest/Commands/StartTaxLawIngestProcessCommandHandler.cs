using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Liara.Azure.Storage;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Palaven.Model;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class StartTaxLawIngestProcessCommandHandler : ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument>
{
    private readonly IBlobStorageService _storageService;
    private readonly IDocumentRepository<EtlTaskDocument> _repository;
    private readonly AzureStorageOptions _azureStorageOptions;
    private readonly QueueClient _documentAnalysisQueue;

    public StartTaxLawIngestProcessCommandHandler(IOptions<AzureStorageOptions> azureStorageOptions, IAzureClientFactory<BlobServiceClient> blobServiceClientFactory, 
        IAzureClientFactory<QueueServiceClient> queueServiceClientFactory,
        IDocumentRepository<EtlTaskDocument> repository)
    {
        ArgumentNullException.ThrowIfNull(blobServiceClientFactory, nameof(blobServiceClientFactory));        

        _azureStorageOptions = azureStorageOptions?.Value ?? throw new ArgumentNullException(nameof(azureStorageOptions));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        var blobServiceClient = blobServiceClientFactory.CreateClient("AzureStorageLawDocs") ?? 
            throw new InvalidOperationException("Unable to create BlobServiceClient");
                
        _storageService = new BlobStorageService(blobServiceClient);

        var queueServiceClient = queueServiceClientFactory.CreateClient("AzureStorageLawDocs") ??
            throw new InvalidOperationException("Unable to create QueueServiceClient");

        _documentAnalysisQueue = queueServiceClient.GetQueueClient(_azureStorageOptions.QueueNames[QueueStorageNames.DocumentAnalysisQueue]);
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return await Task.FromResult(Result<EtlTaskDocument>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) }));
        }

        var etlTask = CreateEtlTaskDocument(command);
        var result = await _repository.CreateAsync(etlTask, new PartitionKey(etlTask.UserId.ToString()), itemRequestOptions: null, cancellationToken);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Unable to create {nameof(StartTaxLawIngestTaskDocument)} document. Status code: {result.StatusCode}");
        }

        var uploadBlobModel = CreateAppendBlobModel(command, etlTask);
        await _storageService.AppendAsync(uploadBlobModel, cancellationToken);

        var documentAnalysisMessage = CreateDocumentAnalysisMessage(etlTask);
        await _documentAnalysisQueue.SendMessageAsync(JsonConvert.SerializeObject(documentAnalysisMessage), cancellationToken: cancellationToken);

        return new Result<EtlTaskDocument> { Value = etlTask };
    }

    private EtlTaskDocument CreateEtlTaskDocument(StartTaxLawIngestCommand command)
    {
        var documentId = Guid.NewGuid();        
        
        var etlTask = new EtlTaskDocument
        {
            Id = documentId.ToString(),
            TenantId = command.UserId,
            UserId = command.UserId,
            DocumentSchema = nameof(EtlTaskDocument),
            IsTaskCompleted = false
        };


        var fileName = $"{documentId}{command.FileExtension}";

        etlTask.Metadata.Add(EtlMetadataKeys.FileName, fileName);
        etlTask.Metadata.Add(EtlMetadataKeys.FileUntrustedName, command.UntrustedFileName);
        etlTask.Metadata.Add(EtlMetadataKeys.FileContentLocale, "es");
        etlTask.Metadata.Add(EtlMetadataKeys.LawAcronym, command.AcronymLaw);
        etlTask.Metadata.Add(EtlMetadataKeys.LawName, command.NameLaw);
        etlTask.Metadata.Add(EtlMetadataKeys.LawYear, command.YearLaw.ToString());

        return etlTask;
    }

    private AppendBlobModel CreateAppendBlobModel(StartTaxLawIngestCommand command, EtlTaskDocument etlTask)
    {
        return new AppendBlobModel
        {
            BlobContainerName = _azureStorageOptions.BlobContainers[BlobStorageContainers.EtlInbox],
            BlobContent = command.FileContent,
            BlobName = etlTask.Metadata[EtlMetadataKeys.FileName],
            CreateOptions = new AppendBlobCreateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "OperationId", etlTask.Id }
                }
            }
        };
    }

    private DocumentAnalysisMessage CreateDocumentAnalysisMessage(EtlTaskDocument etlTask)
    {
        return new DocumentAnalysisMessage
        {
            OperationId = etlTask.Id,
            Locale = etlTask.Metadata[EtlMetadataKeys.FileContentLocale],
            DocumentBlobName = etlTask.Metadata[EtlMetadataKeys.FileName]
        };
    }
}