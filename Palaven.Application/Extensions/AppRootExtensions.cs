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
using Palaven.Application.DatasetsManagement;

namespace Palaven.Application.Extensions;

public static class AppRootExtensions
{
    public static IServiceCollection AddDatasetManagementServices(this IServiceCollection services)
    {
        services.AddTransient<ICommandHandler<CreateInstructionDatasetCommand>, CreateInstructionDatasetCommandHandler>();
        services.AddTransient<IFineTuningDatasetService,FineTuningDatasetService>();
        services.AddTransient<IInstructionDatasetService, InstructionDatasetService>();
        services.AddTransient<ICreateInstructionDatasetChoreographyService, CreateInstructionDatasetChoreographyService>();

        return services;
    }

    public static IServiceCollection AddPerformanceEvaluationServices(this IServiceCollection services)
    {
        services.AddTransient<ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo>, CreateEvaluationSessionCommandHandler>(); 
        services.AddTransient<ICommandHandler<CleanChatCompletionResponseCommand>, CleanChatCompletionResponseCommandHandler>();        
        services.AddTransient<ICommandHandler<UpsertChatCompletionResponseCommand>, UpsertChatCompletionResponseCommandHandler>();                
        services.AddTransient<IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>>, LlmChatCompletionResponseQueryHandler>();
        services.AddTransient<IPerformanceEvaluationService, PerformanceEvaluationService>();

        return services;
    }

    public static IServiceCollection AddPerformanceMetricsService(this IServiceCollection services)
    {
        services.AddTransient<IPerformanceMetricsService, PerformanceMetricsService>();

        return services;
    }
}
