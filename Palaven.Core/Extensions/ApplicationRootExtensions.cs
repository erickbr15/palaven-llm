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
        services.AddTransient<ICommandHandler<UpsertChatCompletionResponseCommand>, UpsertChatCompletionResponseCommandHandler>();
        services.AddTransient<ICommandHandler<CleanChatCompletionResponseCommand>, CleanChatCompletionResponseCommandHandler>();
        services.AddTransient<IQueryHandler<LlmChatCompletionResponseQuery, IEnumerable<LlmResponseView>>, LlmChatCompletionResponseQueryHandler>();
        services.AddTransient<IInstructionDatasetService, InstructionDatasetService>();
        services.AddTransient<IFineTuningDatasetService, FineTuningDatasetService>();
        services.AddTransient<IPerformanceEvaluationService, PerformanceEvaluationService>();

        return services;
    }
}
