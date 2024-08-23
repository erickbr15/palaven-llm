using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class QueryLlmChatCompletionResponses : IQueryCommand<SearchLlmChatCompletionResponseCriteria, IList<LlmResponseView>>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public QueryLlmChatCompletionResponses(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public IResult<IList<LlmResponseView>> Search(SearchLlmChatCompletionResponseCriteria criteria)
    {
        if(criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        IList<LlmResponseView> result = new List<LlmResponseView>();
        if(string.Equals(criteria.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmVanilla, StringComparison.OrdinalIgnoreCase))
        {
            result = _performanceEvaluationDataService.FetchChatCompletionLlmResponses(criteria.SelectionCriteria);
        }
        else if(string.Equals(criteria.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmWithRag, StringComparison.OrdinalIgnoreCase))
        {
            result = _performanceEvaluationDataService.FetchChatCompletionLlmWithRagResponses(criteria.SelectionCriteria);
        }
        
        return Result<IList<LlmResponseView>>.Success(result);
    }
}
