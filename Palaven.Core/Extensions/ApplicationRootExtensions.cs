using Liara.Common;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Core.Datasets;
using Palaven.Core.PerformanceEvaluation;
using Palaven.Core.PerformanceEvaluation.Commands;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.Extensions;

public static class ApplicationRootExtensions
{
    public static IServiceCollection AddPalavenCoreServices(this IServiceCollection services)
    {
        services.AddTransient<ICommand<IEnumerable<UpsertChatCompletionResponseModel>, bool>, UpsertChatCompletionResponseCommand>();
        services.AddTransient<ICommand<CleanChatCompletionResponsesModel, bool>, CleanChatCompletionResponsesCommand>();
        services.AddTransient<IQueryCommand<SearchLlmChatCompletionResponseCriteria, IList<LlmResponseView>>, QueryLlmChatCompletionResponses>();
        services.AddTransient<IInstructionDatasetService, InstructionDatasetService>();
        services.AddTransient<IFineTuningDatasetService, FineTuningDatasetService>();
        services.AddTransient<IPerformanceEvaluationService, PerformanceEvaluationService>();

        return services;
    }
}
