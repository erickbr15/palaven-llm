using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Liara.Common.Abstractions;
using Palaven.Infrastructure.Abstractions.AI;
using Palaven.Infrastructure.Model.AI;
using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;
using Liara.Common;

namespace Palaven.Infrastructure.MicrosoftAzure.AI;

public class PdfDocumentAnalyzer : IPdfDocumentAnalyzer
{
    private readonly DocumentAnalysisClient _documentAnalysisClient;

    public PdfDocumentAnalyzer(DocumentAnalysisClient documentAnalysisClient)
    {
        _documentAnalysisClient = documentAnalysisClient ?? throw new ArgumentNullException(nameof(documentAnalysisClient));
    }

    public async Task<string> AnalyzeDocumentAsync(Stream documentContent, string documentLocale, IList<string> documentPages, CancellationToken cancellationToken)
    {
        var documentAnalysisOptions = new AnalyzeDocumentOptions();

        documentAnalysisOptions.Locale = documentLocale;
        documentAnalysisOptions.Pages.ToList().AddRange(documentPages);

        var result = await _documentAnalysisClient.AnalyzeDocumentAsync(WaitUntil.Started, "prebuilt-layout", documentContent, documentAnalysisOptions, cancellationToken);

        return result.Id;
    }

    public async Task<IResult<DocumentAnalysisResult>> GetDocumentAnalysisResultAsync(string analysisOperationId, CancellationToken cancellationToken)
    {
        var result = new DocumentAnalysisResult();
        try
        {
            result.DocumentAnalysisOperationId = analysisOperationId;
            var documentAnalysisOperation = await HydrateAnalyzedDocumentOperationAsync(analysisOperationId, cancellationToken);

            if (!documentAnalysisOperation.HasCompleted)
            {
                result.IsComplete = false;
                return Result<DocumentAnalysisResult>.Success(result);
            }

            result.IsComplete = true;
            result.Paragraphs = GetRelevantParagraphs(documentAnalysisOperation.Value.Paragraphs.ToArray());

            return Result<DocumentAnalysisResult>.Success(result);
        }
        catch(Exception ex)
        {
            return Result<DocumentAnalysisResult>.Fail(new List<ValidationError>(), new List<Exception> { ex });
        }        
    }

    private async Task<AnalyzeDocumentOperation> HydrateAnalyzedDocumentOperationAsync(string analysisOperationId, CancellationToken cancellationToken)
    {
        var documentAnalysisOperation = new AnalyzeDocumentOperation(analysisOperationId, _documentAnalysisClient);
        await documentAnalysisOperation.UpdateStatusAsync(cancellationToken);

        return documentAnalysisOperation;
    }

    private TaxLawDocumentParagraph[] GetRelevantParagraphs(DocumentParagraph[] paragraphs)
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

        var relevantParagraphs = new List<DocumentParagraph>();
        
        relevantParagraphs.AddRange(paragraphs.Where(p => !p.Role.HasValue));
        relevantParagraphs.AddRange(paragraphs.Where(p => p.Role.HasValue && !noneRelevantParagraphs.Contains(p.Role.Value)));        

        var paragraphsToReturn = new List<TaxLawDocumentParagraph>();

        foreach (var paragraph in relevantParagraphs)
        {
            var newParagraph = new TaxLawDocumentParagraph
            {
                ParagraphId = Guid.NewGuid(),
                Content = paragraph.Content,
                PageNumber = paragraph.BoundingRegions.Any() ? paragraph.BoundingRegions[0].PageNumber : -1
            };

            newParagraph.Spans.AddRange(paragraph.Spans.Select(s => new TaxLawDocumentSpan
            {
                Index = s.Index,
                Length = s.Length
            }));

            newParagraph.BoundingBoxes.AddRange(paragraph.BoundingRegions.SelectMany(b => b.BoundingPolygon));

            paragraphsToReturn.Add(newParagraph);
        }

        return paragraphsToReturn.ToArray();
    }
}
