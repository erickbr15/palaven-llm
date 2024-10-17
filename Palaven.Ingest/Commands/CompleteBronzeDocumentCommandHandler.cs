using Azure.AI.FormRecognizer.DocumentAnalysis;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Data.Documents;
using Palaven.Model.Data.Documents.Metadata;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class CompleteBronzeDocumentCommandHandler : ICommandHandler<CompleteBronzeDocumentCommand, EtlTaskDocument>
{
    private readonly DocumentAnalysisClient _documentAnalysisClient;
    private readonly IDocumentRepository<EtlTaskDocument> _taskDocumentRepository;
    private readonly IDocumentRepository<BronzeDocument> _bronzeDocumentRepository;    

    public CompleteBronzeDocumentCommandHandler(DocumentAnalysisClient documentAnalysisClient, IDocumentRepository<EtlTaskDocument> taskDocumentRepository,
        IDocumentRepository<BronzeDocument> bronzeDocumentRepository)
    {
        
        _documentAnalysisClient = documentAnalysisClient ?? throw new ArgumentNullException(nameof(documentAnalysisClient));
        _taskDocumentRepository = taskDocumentRepository ?? throw new ArgumentNullException(nameof(taskDocumentRepository));
        _bronzeDocumentRepository = bronzeDocumentRepository ?? throw new ArgumentNullException(nameof(bronzeDocumentRepository));
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(CompleteBronzeDocumentCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return Result<EtlTaskDocument>.Fail(new List<ValidationError>(), 
                new List<Exception> { new ArgumentNullException(nameof(command)) });
        }

        var etlTask = await FetchLatestEtlTaskVersionAsync(command.OperationId, cancellationToken);

        var documentAnalysisOperation = await HydrateAnalyzedDocumentOperationAsync(command.DocumentAnalysisOperationId, cancellationToken);
        if (!documentAnalysisOperation.HasCompleted)
        {
            return Result<EtlTaskDocument>.Success(etlTask!);
        }
                
        var bronzeDocuments = CreateBronzeDocuments(etlTask!, documentAnalysisOperation.Value.Paragraphs.ToArray());
        
        var (succesfulUpserts, failedUpserts) = await SaveBronzeDocumentsAsync(bronzeDocuments, cancellationToken);
        
        PopulateBronzeDocumentMetadataAndDetails(etlTask!, documentAnalysisOperation.Value.Pages.Count, succesfulUpserts, failedUpserts);
        
        var dbResponse = await _taskDocumentRepository.UpsertAsync(etlTask!, new PartitionKey(etlTask!.TenantId.ToString()), 
            itemRequestOptions: null, cancellationToken: cancellationToken);

        if(dbResponse.StatusCode != HttpStatusCode.OK)
        {
            var result = Result<EtlTaskDocument>.Fail(new List<ValidationError>(),
                new List<Exception> { new InvalidOperationException($"Unable to update the ETL task document. Status code: {dbResponse.StatusCode}") });
            result.Value = etlTask!;
            return result;
        }

        return Result<EtlTaskDocument>.Success(etlTask);
    } 

    private async Task<EtlTaskDocument?> FetchLatestEtlTaskVersionAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = \"{operationId}\"");

        var queryResults = await _taskDocumentRepository.GetAsync(
            query,
            continuationToken: null,
            queryRequestOptions: null,
            cancellationToken);

        return queryResults.SingleOrDefault();
    }

    private async Task<AnalyzeDocumentOperation> HydrateAnalyzedDocumentOperationAsync(string documentAnalysisOperationId, CancellationToken cancellationToken)
    {
        var documentAnalysisOperation = new AnalyzeDocumentOperation(documentAnalysisOperationId, _documentAnalysisClient);        
        await documentAnalysisOperation.UpdateStatusAsync(cancellationToken);

        return documentAnalysisOperation;
    }

    private async Task<(int SuccessfulUpserts, int FailedUpserts)> SaveBronzeDocumentsAsync(IList<BronzeDocument> bronzeDocuments, CancellationToken cancellationToken)
    {
        int successfulUpserts = 0;
        int failedUpserts = 0;

        foreach (var document in bronzeDocuments)
        {
            try
            {
                var dbResponse = await _bronzeDocumentRepository.UpsertAsync(document, new PartitionKey(document.TenantId.ToString()), itemRequestOptions: null,
                    cancellationToken: cancellationToken);

                if(dbResponse.StatusCode != HttpStatusCode.Created && dbResponse.StatusCode != HttpStatusCode.OK)
                {
                    failedUpserts++;
                }
                else
                {
                    successfulUpserts++;
                }                    
            }
            catch
            {
                //TODO: Log the error
                failedUpserts++;
            }
        }

        return (successfulUpserts, failedUpserts);
    }    

    private void PopulateBronzeDocumentMetadataAndDetails(EtlTaskDocument document, int detectedPageCount, int ingestedPageCount, int ingestPageErrorCount)
    {
        document.Metadata.Add(EtlMetadataKeys.BronzeLayerProcessed, true.ToString());
        document.Metadata.Add(EtlMetadataKeys.BronzeLayerCompleted, (detectedPageCount == ingestedPageCount).ToString());
        document.Metadata.Add(EtlMetadataKeys.BronzeLayerDetectedPageCount, detectedPageCount.ToString());
        document.Metadata.Add(EtlMetadataKeys.BronzeLayerIngestedPageCount, ingestedPageCount.ToString());

        document.Details.Add($"{DateTime.UtcNow.ToString()}. Bronze layer processed. Detected pages: {detectedPageCount}. Ingested pages: {ingestedPageCount}. Ingest page errors: {ingestPageErrorCount}");
    }    

    private static IList<BronzeDocument> CreateBronzeDocuments(EtlTaskDocument etlTask, DocumentParagraph[] paragraphs)
    {
        var noneRelevantParagraphs = new List<ParagraphRole>
        {
            ParagraphRole.FormulaBlock,
            ParagraphRole.SectionHeading,
            ParagraphRole.PageHeader,
            ParagraphRole.PageFooter,
            ParagraphRole.Title,
            ParagraphRole.Footnote,
            ParagraphRole.PageNumber
        };

        var relevantTaskMetadataKeys = new List<string>
        {
            EtlMetadataKeys.LawName,
            EtlMetadataKeys.LawYear,
            EtlMetadataKeys.LawAcronym,
            EtlMetadataKeys.FileName,
            EtlMetadataKeys.FileContentLocale,
            EtlMetadataKeys.FileUntrustedName            
        };

        var relevantParagraphs = new List<DocumentParagraph>();
        relevantParagraphs.AddRange(paragraphs.Where(p => !p.Role.HasValue));
        relevantParagraphs.AddRange(paragraphs.Where(p => p.Role.HasValue && !noneRelevantParagraphs.Contains(p.Role.Value)));

        var minPageNumber = relevantParagraphs.SelectMany(p => p.BoundingRegions).Min(b => b.PageNumber);
        var maxPageNumber = relevantParagraphs.SelectMany(p => p.BoundingRegions).Max(b => b.PageNumber);

        var documents = new List<BronzeDocument>();

        for (int pageNumber = minPageNumber; pageNumber <= maxPageNumber; pageNumber++)
        {
            var relevantParagraphsOnPage = relevantParagraphs.Where(r => r.BoundingRegions.Any(r => r.PageNumber == pageNumber)).ToArray();

            var newDocument = new BronzeDocument
            {
                Id = Guid.NewGuid(),
                TenantId = etlTask.TenantId,
                TraceId = new Guid(etlTask.Id),
                DocumentSchema = nameof(BronzeDocument),
                PageNumber = pageNumber
            };

            foreach (var key in relevantTaskMetadataKeys)
            {
                newDocument.Metadata.Add(key, etlTask.Metadata[key]);
            }

            foreach (var paragraph in relevantParagraphsOnPage)
            {
                var bronzeParagraph = new TaxLawDocumentParagraph
                {
                    ParagraphId = Guid.NewGuid(),
                    DocumentId = newDocument.Id,
                    PageNumber = pageNumber,
                    Content = paragraph.Content
                };

                bronzeParagraph.Spans.AddRange(paragraph.Spans.Select(s => new TaxLawDocumentSpan
                {
                    Index = s.Index,
                    Length = s.Length
                }));

                bronzeParagraph.BoundingBoxes.AddRange(paragraph.BoundingRegions
                    .Where(b => b.PageNumber == pageNumber)
                    .SelectMany(b => b.BoundingPolygon));

                newDocument.Paragraphs.Add(bronzeParagraph);
            }

            documents.Add(newDocument);
        }

        return documents;
    }
}