using Liara.Common;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation;

public interface IPerformanceEvaluationService
{
    Task<IResult<EvaluationSessionInfo>> CreateEvaluationSessionAsync(CreateEvaluationSessionModel model, CancellationToken cancellationToken);         
    Task<IResult> UpsertChatCompletionResponseAsync(IEnumerable<UpsertChatCompletionResponseModel> model, CancellationToken cancellationToken);
    Task<IResult> UpsertChatCompletionPerformanceEvaluationAsync(UpsertChatCompletionPerformanceEvaluationModel model, CancellationToken cancellationToken);
}
