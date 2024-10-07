using Liara.Azure.BlobStorage;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class StartTaxLawIngestCommandHandler : ICommandHandler<IngestTaxLawDocumentCommand, TaxLawDocumentIngestTask>
{
    private readonly IBlobStorageService _storageService;
    private readonly IDocumentRepository<TaxLawToIngestDocument> _repository;

    public StartTaxLawIngestCommandHandler(IBlobStorageService blobStorageService, IDocumentRepository<TaxLawToIngestDocument> repository)
    {
        _storageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IResult<TaxLawDocumentIngestTask>> ExecuteAsync(IngestTaxLawDocumentCommand command, CancellationToken cancellationToken)
    {        
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");
        var documentId = Guid.NewGuid();

        var persistedFileName = $"{documentId}{command.FileExtension}";

        var uploadBlobModel = new BlobUploadModel
        {
            BlobContent = command.FileContent,
            BlobName = persistedFileName
        };

        await _storageService.AppendAsync(uploadBlobModel, cancellationToken);

        var lawToIngestDocument = new TaxLawToIngestDocument
        {
            Id = documentId.ToString(),
            TenantId = tenantId.ToString(),
            TraceId = command.TraceId,
            FileName = persistedFileName,
            OriginalFileName = command.FileName,
            LawId = Guid.NewGuid(),
            AcronymLaw = command.Acronym,
            NameLaw = command.Name,
            YearLaw = command.Year,
            StartPageExtractionData = command.StartPageExtractionData,
            ChunkSizeExtractionData = command.ChunkSizeExtractionData,
            TotalNumberOfPages = command.TotalNumberOfPages,
            LawDocumentVersion = command.LawDocumentVersion,
            DocumentType = nameof(TaxLawToIngestDocument)
        };

        var result = await _repository.CreateAsync(lawToIngestDocument, new PartitionKey(tenantId.ToString()), itemRequestOptions: null, cancellationToken);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Unable to create the TaxLawToIngest document. Status code: {result.StatusCode}");
        }

        return new Result<TaxLawDocumentIngestTask> { Value = new TaxLawDocumentIngestTask { TraceId = command.TraceId } };
    }    
}
