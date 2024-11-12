using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Palaven.Persistence.EntityFramework;
using Liara.Persistence.Abstractions;
using Palaven.Persistence.EntityFramework.Repositories;
using Palaven.Infrastructure.Model.Persistence.Entities;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Persistence.EntityFramework.Services;

namespace Palaven.Data.Sql.Extensions;

public static class ApplicationRootExtensions
{    
    public static void AddDataSqlServices(this IServiceCollection services, string connectionString)
    {        
        services.AddDbContext<PalavenDbContext>(options => {            
            options.UseSqlServer(connectionString);
        });
                
        services.AddTransient<IRepository<EvaluationSession>, EvaluationSessionRepository>();
        services.AddTransient<IRepository<LlmResponse>, LlmResponseRepository>();
        services.AddTransient<IRepository<InstructionEntity>, InstructionRepository>();
        services.AddTransient<IRepository<BertScoreMetric>, BertScoreMetricRepository>();
        services.AddTransient<IRepository<RougeScoreMetric>, RougeScoreMetricRepository>();
        services.AddTransient<IRepository<FineTuningPromptEntity>, FineTuningPromptRepository>();
        services.AddTransient<IRepository<EvaluationSessionInstruction>, EvaluationSessionInstructionRepository>();
        services.AddTransient<IRepository<BleuMetric>, BleuMetricRepository>();
        services.AddTransient<IDatasetsDataService, DatasetsDataService>();
        services.AddTransient<IEvaluationSessionDataService, PerformanceEvaluationDataService>();
        services.AddTransient<IPerformanceMetricDataService, PerformanceMetricsDataService>();
    }
}
