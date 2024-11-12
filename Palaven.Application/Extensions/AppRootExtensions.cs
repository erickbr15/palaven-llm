using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Application.PerformanceEvaluation.Commands;
using Palaven.Application.PerformanceEvaluation.Queries;
using Palaven.Infrastructure.Model.Persistence.Views;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Application.DatasetManagement.Commands;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.DatasetManagement;
using Palaven.Application.PerformanceEvaluation;
using Palaven.Application.Abstractions.PerformanceMetrics;
using Palaven.Application.PerformanceMetrics;

namespace Palaven.Application.Extensions;

public static class AppRootExtensions
{
    public static IServiceCollection AddDatasetManagementServices(this IServiceCollection services)
    {
        services.AddSingleton<ICommandHandler<CreateInstructionDatasetCommand>, CreateInstructionDatasetCommandHandler>();
        services.AddSingleton<IFineTuningDatasetService,FineTuningDatasetService>();
        services.AddSingleton<IInstructionDatasetService, InstructionDatasetService>();

        return services;
    }

    public static IServiceCollection AddPerformanceEvaluationServices(this IServiceCollection services)
    {
        services.AddSingleton<ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo>, CreateEvaluationSessionCommandHandler>(); 
        services.AddSingleton<ICommandHandler<CleanChatCompletionResponseCommand>, CleanChatCompletionResponseCommandHandler>();        
        services.AddSingleton<ICommandHandler<UpsertChatCompletionResponseCommand>, UpsertChatCompletionResponseCommandHandler>();                
        services.AddSingleton<IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>>, LlmChatCompletionResponseQueryHandler>();
        services.AddSingleton<IPerformanceEvaluationService, PerformanceEvaluationService>();

        return services;
    }

    public static IServiceCollection AddPerformanceMetricsService(this IServiceCollection services)
    {
        services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();

        return services;
    }
}
