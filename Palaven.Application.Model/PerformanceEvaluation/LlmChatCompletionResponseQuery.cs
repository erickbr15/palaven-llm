using Palaven.Infrastructure.Model.Persistence.Views;

namespace Palaven.Application.Model.PerformanceEvaluation;

public class LlmChatCompletionResponseQuery
{
    public Func<LlmResponseView, bool> SelectionCriteria { get; set; } = default!;
}
