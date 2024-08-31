using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Liara.Common.DataAccess;
using Palaven.Data.Sql.Repositories;
using Palaven.Data.Sql.Services;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Entities;

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
        services.AddTransient<IDatasetsDataService, DatasetsDataService>();
        services.AddTransient<IPerformanceEvaluationDataService, PerformanceEvaluationDataService>();
        services.AddTransient<IPerformanceMetricsDataService, PerformanceMetricsDataService>();
    }
}
