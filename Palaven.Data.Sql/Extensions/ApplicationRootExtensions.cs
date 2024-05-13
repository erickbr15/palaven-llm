using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Liara.Common.DataAccess;
using Palaven.Model.Datasets;
using Palaven.Data.Sql.Repositories;
using Palaven.Model.LllmPerformance;
using Palaven.Data.Sql.Services;
using Palaven.Data.Sql.Services.Contracts;

namespace Palaven.Data.Sql.Extensions;

public static class ApplicationRootExtensions
{    
    public static void AddDataSqlServices(this IServiceCollection services)
    {
        services.AddDbContext<PalavenDbContext>(options => {
            options.UseSqlServer("name=ConnectionStrings:PalavenDb");
        });

        services.AddTransient<IRepository<Instruction>, InstructionRepository>();
        services.AddTransient<IRepository<BertScoreEvaluationMetric>, BertScoreEvaluationMetricsRepository>();
        services.AddTransient<IRepository<RagBertScoreEvaluationMetric>, RagBertScoreEvaluationMetricsRepository>();
        services.AddTransient<IRepository<FineTuningBertScoreEvaluationMetric>, FineTuningBertScoreEvaluationMetricsRepository>();
        services.AddTransient<IRepository<RagFineTuningBertScoreEvaluationMetric>, RagFineTuningBertScoreEvaluationMetricsRepository>();
        services.AddTransient<IInstructionDataService, InstructionDataService>();
        services.AddTransient<IBertScoreEvaluationMetricsDataService, BertScoreEvaluationMetricsDataService>();
        services.AddTransient<IRagBertScoreMetricsDataService, RagBertScoreMetricsDataService>();
        services.AddTransient<IFineTuningBertScoreMetricsDataService, FineTuningBertScoreMetricsDataService>();
        services.AddTransient<IRagFineTuningBertScoreMetricsDataService, RagFineTuningBertScoreMetricsDataService>();
    }
}
