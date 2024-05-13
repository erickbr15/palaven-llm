using Microsoft.EntityFrameworkCore;
using Palaven.Model.Datasets;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql;

public interface IPalavenDbContext
{
    DbSet<Instruction> Instructions { get; set; }
    DbSet<BertScoreEvaluationMetric> BertScoreEvaluationMetrics { get; set; }
}
