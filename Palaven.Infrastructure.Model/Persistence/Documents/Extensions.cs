using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Infrastructure.Model.Persistence.Documents;

public static class Extensions
{
    public static DocumentAnalysisMessage ToDocumentAnalysisMessage(this EtlTaskDocument taskDocument)
    {
        return new DocumentAnalysisMessage
        {
            OperationId = taskDocument.Id,
            DocumentBlobName = taskDocument.Metadata[EtlMetadataKeys.FileName],
            Locale = taskDocument.Metadata[EtlMetadataKeys.FileContentLocale]
        };
    }
}
