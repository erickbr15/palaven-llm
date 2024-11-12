using Liara.Common.Abstractions;
using Palaven.Infrastructure.Model.AI;

namespace Palaven.Infrastructure.Abstractions.AI;

public interface IPdfDocumentAnalyzer
{
    Task<string> AnalyzeDocumentAsync(Stream documentContent, string documentLocale, IList<string> documentPages, CancellationToken cancellationToken);
    Task<IResult<DocumentAnalysisResult>> GetDocumentAnalysisResultAsync(string analysisOperationId, CancellationToken cancellationToken);
}
