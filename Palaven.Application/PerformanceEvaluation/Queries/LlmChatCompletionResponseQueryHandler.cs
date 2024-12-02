using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.Persistence.Views;

namespace Palaven.Application.PerformanceEvaluation.Queries;

public class LlmChatCompletionResponseQueryHandler : IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>>
{
    private readonly IEvaluationSessionDataService _evaluationSessionDataService;

    public LlmChatCompletionResponseQueryHandler(IEvaluationSessionDataService evaluationSessionDataService)
    {
        _evaluationSessionDataService = evaluationSessionDataService ?? throw new ArgumentNullException(nameof(evaluationSessionDataService));
    }

    public Task<IResult<IList<LlmResponseView>>> ExecuteAsync(LlmChatCompletionResponseQuery query, CancellationToken cancellationToken)
    {
        if (query == null)
        {
            return Task.FromResult(Result<IList<LlmResponseView>>.Fail(new ArgumentNullException(nameof(query))));
        }

        var result = _evaluationSessionDataService.FetchChatCompletionLlmResponses(query.SelectionCriteria);

        return Task.FromResult(Result<IList<LlmResponseView>>.Success(result));
    }
}
