using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;

namespace Palaven.Infrastructure.Model.AI;

public class DocumentAnalysisResult
{
    public string DocumentAnalysisOperationId { get; set; } = default!;
    public bool IsComplete { get; set; }
    public TaxLawDocumentParagraph[] Paragraphs { get; set; } = Array.Empty<TaxLawDocumentParagraph>();
    public EtlTaskDocument EtlRelatedTask { get; set; } = default!;
}
