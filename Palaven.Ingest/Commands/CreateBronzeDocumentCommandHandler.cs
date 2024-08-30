using Azure.AI.FormRecognizer.DocumentAnalysis;
using Liara.Azure.AI;
using Liara.Azure.BlobStorage;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Palaven.Model.Documents;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class CreateBronzeDocumentCommandHandler : ICommandHandler<CreateBronzeDocumentCommand, TaxLawDocumentIngestTask>
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IDocumentRepository<TaxLawToIngestDocument> _lawDocumentToIngestRepository;
    private readonly IDocumentRepository<TaxLawDocumentPage> _lawPageDocumentRepository;
    private readonly IDocumentLayoutAnalyzerService _documentAnalyzer;

    public CreateBronzeDocumentCommandHandler(
        IOptions<BlobStorageConnectionOptions> storageOptions,
        IDocumentRepository<TaxLawToIngestDocument> lawDocumentToIngestRepository,
        IDocumentRepository<TaxLawDocumentPage> lawPageDocumentRepository,
        IDocumentLayoutAnalyzerService documentAnalyzer)
    {   
        var options = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));

        var blobContainerName = options.Containers.TryGetValue(BlobStorageIngestContainers.LawDocsV1, out var containerName) ?
            containerName :
            throw new InvalidOperationException($"Unable to find the container name {BlobStorageIngestContainers.LawDocsV1}");

        _blobStorageService = new BlobStorageService(options.ConnectionString, blobContainerName);
        _lawDocumentToIngestRepository = lawDocumentToIngestRepository ?? throw new ArgumentNullException(nameof(lawDocumentToIngestRepository));
        _lawPageDocumentRepository = lawPageDocumentRepository ?? throw new ArgumentNullException(nameof(lawPageDocumentRepository));
        _documentAnalyzer = documentAnalyzer ?? throw new ArgumentNullException(nameof(documentAnalyzer));
    }

    public async Task<IResult<TaxLawDocumentIngestTask>> ExecuteAsync(CreateBronzeDocumentCommand command, CancellationToken cancellationToken)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{command.TraceId}\"");

        var queryResults = await _lawDocumentToIngestRepository.GetAsync(
            query, 
            continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },            
            cancellationToken);
        
        var taxLawToIngestDocument = queryResults.SingleOrDefault() ?? throw new InvalidOperationException($"Unable to find the tax law document to ingest with trace id {command.TraceId}");

        var latestPage = await GetLastestExtractedPageAsync(command.TraceId, cancellationToken);

        await ExtractDocumentPagesAsync(command.TraceId, taxLawToIngestDocument, latestPage?.PageNumber, cancellationToken);
                                           
        return new Result<TaxLawDocumentIngestTask> { Value = new TaxLawDocumentIngestTask { TraceId = command.TraceId } };
    }

    private async Task<TaxLawDocumentPage?> GetLastestExtractedPageAsync(Guid traceId, CancellationToken cancellationToken)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{traceId}\"");

        var queryResults = await _lawPageDocumentRepository.GetAsync(
            query,
            continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken);

        var lastestPage = queryResults.OrderByDescending(p => p.PageNumber).FirstOrDefault();

        return lastestPage;
    }

    private async Task ExtractDocumentPagesAsync(Guid traceId, TaxLawToIngestDocument taxLawToIngestDocument, int? latestExtractedPageNumber, CancellationToken cancellationToken)
    {        
        var documentContent = await _blobStorageService.ReadAsync(taxLawToIngestDocument.FileName, cancellationToken);
                
        var startingPage = latestExtractedPageNumber.HasValue ? latestExtractedPageNumber.Value + 1 : 1;

        for (var currentPage = startingPage; currentPage <= taxLawToIngestDocument.TotalNumberOfPages; currentPage += taxLawToIngestDocument.ChunkSizeExtractionData)
        {
            var endPage = currentPage + taxLawToIngestDocument.ChunkSizeExtractionData - 1;
            await ExtractAndSavePageChunkAsync(traceId, taxLawToIngestDocument, documentContent, startPage: currentPage, endPage, cancellationToken);
        }
    }

    private async Task ExtractAndSavePageChunkAsync(Guid traceId, TaxLawToIngestDocument taxLawToIngestDocument, byte[] documentContent, int startPage, int endPage, CancellationToken cancellationToken)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");
        var analysisOptions = new AnalyzeDocumentOptions
        {
            Pages = { $"{startPage}-{endPage}" }
        };

        var documentPages = await _documentAnalyzer.GetPagesAsync(documentContent, analysisOptions, cancellationToken);

        foreach (var page in documentPages)
        {
            var taxLawPage = BuildTaxLawDocumentPage(traceId, taxLawToIngestDocument, page);

            var result = await _lawPageDocumentRepository.CreateAsync(taxLawPage,
                new PartitionKey(tenantId.ToString()),
                itemRequestOptions: null,
                cancellationToken);

            if (result.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException($"Unable to create the account file document. Status code: {result.StatusCode}");
            }
        }
    }

    private TaxLawDocumentPage BuildTaxLawDocumentPage(Guid traceId, TaxLawToIngestDocument taxLawToIngestDocument, DocumentPage page)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

        var taxLawPage = new TaxLawDocumentPage
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId.ToString(),
            TraceId = traceId,
            DocumentType = nameof(TaxLawDocumentPage),
            LawDocumentVersion = taxLawToIngestDocument.LawDocumentVersion,
            LawId = taxLawToIngestDocument.LawId,
            PageNumber = page.PageNumber
        };

        var lineCounter = 0;
        taxLawPage.Lines = page.Lines.Select(l =>
        {
            var line = new TaxLawDocumentLine
            {
                PageDocumentId = new Guid(taxLawPage.Id),
                PageNumber = taxLawPage.PageNumber,
                LineNumber = ++lineCounter,
                LineId = Guid.NewGuid(),
                Content = l.Content
            };

            line.BoundingBox.AddRange(l.BoundingPolygon.ToList());

            return line;
        }).ToList();

        return taxLawPage;
    }    
}
