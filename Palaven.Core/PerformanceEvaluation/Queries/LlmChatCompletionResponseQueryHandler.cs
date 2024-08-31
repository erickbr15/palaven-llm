using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Core.PerformanceEvaluation.Queries;

public class LlmChatCompletionResponseQueryHandler : IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public LlmChatCompletionResponseQueryHandler(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public Task<IResult<IList<LlmResponseView>>> ExecuteAsync(LlmChatCompletionResponseQuery command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var result = _performanceEvaluationDataService.FetchChatCompletionLlmResponses(command.SelectionCriteria);

        return Task.FromResult(Result<IList<LlmResponseView>>.Success(result));
    }
}
