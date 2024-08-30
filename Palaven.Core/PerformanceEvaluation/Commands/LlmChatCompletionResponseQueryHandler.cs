using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class LlmChatCompletionResponseQueryHandler : IQueryHandler<LlmChatCompletionResponseQuery, IEnumerable<LlmResponseView>>  //IQueryCommand<LlmChatCompletionResponseQuery, IList<LlmResponseView>>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public LlmChatCompletionResponseQueryHandler(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public Task<IResult<IEnumerable<LlmResponseView>>> ExecuteAsync(LlmChatCompletionResponseQuery command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        IEnumerable<LlmResponseView> result = new List<LlmResponseView>();

        if (string.Equals(command.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmVanilla, StringComparison.OrdinalIgnoreCase))
        {
            result = _performanceEvaluationDataService.FetchChatCompletionLlmResponses(command.SelectionCriteria);
        }
        else if (string.Equals(command.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmWithRag, StringComparison.OrdinalIgnoreCase))
        {
            result = _performanceEvaluationDataService.FetchChatCompletionLlmWithRagResponses(command.SelectionCriteria);
        }        

        return Task.FromResult(Result<IEnumerable<LlmResponseView>>.Success(result));
    }

    public IResult<IList<LlmResponseView>> Search(LlmChatCompletionResponseQuery command)
    {
        if(command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        IList<LlmResponseView> result = new List<LlmResponseView>();
        if(string.Equals(command.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmVanilla, StringComparison.OrdinalIgnoreCase))
        {
            result = _performanceEvaluationDataService.FetchChatCompletionLlmResponses(command.SelectionCriteria);
        }
        else if(string.Equals(command.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmWithRag, StringComparison.OrdinalIgnoreCase))
        {
            result = _performanceEvaluationDataService.FetchChatCompletionLlmWithRagResponses(command.SelectionCriteria);
        }
        
        return Result<IList<LlmResponseView>>.Success(result);
    }
}
