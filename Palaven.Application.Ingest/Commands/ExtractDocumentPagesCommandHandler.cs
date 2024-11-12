using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Persistence.Abstractions;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;

namespace Palaven.Application.Ingest.Commands;

public class ExtractDocumentPagesCommandHandler : ICommandHandler<ExtractDocumentPagesCommand, EtlTaskDocument>
{
    private readonly IDocumentRepository<EtlTaskDocument> _taskDocumentRepository;
    private readonly IDocumentRepository<BronzeDocument> _bronzeDocumentRepository;    

    public ExtractDocumentPagesCommandHandler(IDocumentRepository<EtlTaskDocument> taskDocumentRepository, IDocumentRepository<BronzeDocument> bronzeDocumentRepository)
    {
        _taskDocumentRepository = taskDocumentRepository ?? throw new ArgumentNullException(nameof(taskDocumentRepository));
        _bronzeDocumentRepository = bronzeDocumentRepository ?? throw new ArgumentNullException(nameof(bronzeDocumentRepository));
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(ExtractDocumentPagesCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return Result<EtlTaskDocument>.Fail(new ArgumentNullException(nameof(command)));
        }

        var etlTask = await _taskDocumentRepository.GetByIdAsync(command.OperationId.ToString(), cancellationToken);
                        
        var bronzeDocuments = CreateBronzeDocuments(etlTask!, command.Paragraphs);
        
        var (succesfulUpserts, failedUpserts) = await SaveBronzeDocumentsAsync(bronzeDocuments, cancellationToken);
        
        var ingestMetadata = CreateBronzeLayerIngestMetadata(bronzeDocuments.Count, succesfulUpserts);

        ingestMetadata.ToList().ForEach(m => etlTask!.Metadata.Add(m.Key, m.Value));

        etlTask.Details.Add($"{DateTime.UtcNow.ToString()}. Bronze layer processed. Detected pages: {command.Paragraphs.Length}. Ingested pages: {succesfulUpserts}. Ingest page errors: {failedUpserts}");

        await _taskDocumentRepository.UpsertAsync(etlTask!, etlTask!.TenantId.ToString(), cancellationToken);

        return Result<EtlTaskDocument>.Success(etlTask);
    }

    private async Task<(int SuccessfulUpserts, int FailedUpserts)> SaveBronzeDocumentsAsync(IList<BronzeDocument> bronzeDocuments, CancellationToken cancellationToken)
    {
        int successfulUpserts = 0;
        int failedUpserts = 0;

        foreach (var document in bronzeDocuments)
        {
            try
            {
                await _bronzeDocumentRepository.UpsertAsync(document, document.TenantId.ToString(), cancellationToken);
                successfulUpserts++;
            }
            catch
            {
                //TODO: Log the error
                failedUpserts++;
            }
        }

        return (successfulUpserts, failedUpserts);
    }

    private static IDictionary<string, string> CreateBronzeLayerIngestMetadata(int detectedPageCount, int ingestedPageCount)
    {
        return new Dictionary<string, string>
        {
            { EtlMetadataKeys.BronzeLayerProcessed, true.ToString() },
            { EtlMetadataKeys.BronzeLayerCompleted, (detectedPageCount == ingestedPageCount).ToString() },
            { EtlMetadataKeys.BronzeLayerDetectedPageCount, detectedPageCount.ToString() },
            { EtlMetadataKeys.BronzeLayerIngestedPageCount, ingestedPageCount.ToString() }
        };
    }

    private static IList<BronzeDocument> CreateBronzeDocuments(EtlTaskDocument etlTask, TaxLawDocumentParagraph[] paragraphs)
    {
        var relevantTaskMetadataKeys = new List<string>
        {
            EtlMetadataKeys.LawName,
            EtlMetadataKeys.LawYear,
            EtlMetadataKeys.LawAcronym,
            EtlMetadataKeys.FileName,
            EtlMetadataKeys.FileContentLocale,
            EtlMetadataKeys.FileUntrustedName
        };

        var minPageNumber = paragraphs.Min(p => p.PageNumber);
        var maxPageNumber = paragraphs.Max(p => p.PageNumber);

        var documents = new List<BronzeDocument>();

        for (int pageNumber = minPageNumber; pageNumber <= maxPageNumber; pageNumber++)
        {
            var relevantParagraphsOnPage = paragraphs.Where(r => r.PageNumber == pageNumber).ToArray();

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
                paragraph.DocumentId = newDocument.Id;
            }

            newDocument.Paragraphs = relevantParagraphsOnPage;

            documents.Add(newDocument);
        }

        return documents;
    }
}