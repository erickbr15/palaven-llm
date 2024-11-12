using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;

namespace Palaven.Application.Model.Ingest;

public class ExtractDocumentPagesCommand
{
    public Guid OperationId { get; set; }
    public TaxLawDocumentParagraph[] Paragraphs { get; set; } = Array.Empty<TaxLawDocumentParagraph>();

}
