using Liara.Common;
using Liara.Common.Abstractions;
using Palaven.Application.Abstractions.PerformanceMetrics;
using Palaven.Application.Model.PerformanceMetrics;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Application.PerformanceMetrics;

public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly IPerformanceMetricDataService _performanceMetricsDataService;

    public PerformanceMetricsService(IPerformanceMetricDataService performanceMetricDataService)
    {
        _performanceMetricsDataService = performanceMetricDataService ?? throw new ArgumentNullException(nameof(performanceMetricDataService));
    }

    public IList<BertScoreMetrics> FetchEvaluationSessionBertscoreMetrics(Guid evaluationSessionId, string evaluationExercise)
    {
        var evaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(evaluationExercise);

        var result = _performanceMetricsDataService.FetchBertScoreMetrics(b => b.SessionId == evaluationSessionId && b.EvaluationExerciseId == evaluationExerciseId);

        var metrics = result.Select(m => new BertScoreMetrics
        {
            EvaluationSessionId = m.SessionId,
            EvaluationExercise = evaluationExercise.ToLower(),
            BatchNumber = m.BatchNumber,
            Precision = m.BertScorePrecision ?? 0,
            Recall = m.BertScoreRecall ?? 0,
            F1 = m.BertScoreF1 ?? 0
        }).ToList();

        return metrics;
    }

    public IList<BleuMetrics> FetchEvaluationSessionBleuMetrics(Guid evaluationSessionId, string evaluationExercise)
    {
        var evaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(evaluationExercise);

        var result = _performanceMetricsDataService.FetchBleuMetrics(b => b.SessionId == evaluationSessionId && b.EvaluationExerciseId == evaluationExerciseId);

        var metrics = result.Select(m => new BleuMetrics
        {
            EvaluationSessionId = m.SessionId,
            EvaluationExercise = evaluationExercise.ToLower(),
            BatchNumber = m.BatchNumber,
            BleuScore = m.BleuScore ?? 0
        }).ToList();

        return metrics;
    }

    public IList<RougeScoreMetrics> FetchEvaluationSessionRougeScoreMetrics(Guid evaluationSessionId, string evaluationExercise, string rougeType)
    {
        var evaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(evaluationExercise);
        
        rougeType = string.IsNullOrWhiteSpace(rougeType) ? "rouge1" : rougeType.ToLower();

        var result = _performanceMetricsDataService.FetchRougeScoreMetrics(b => b.SessionId == evaluationSessionId && b.EvaluationExerciseId == evaluationExerciseId && b.RougeType == rougeType);

        var metrics = result.Select(m => new RougeScoreMetrics
        {
            EvaluationSessionId = m.SessionId,
            EvaluationExercise = evaluationExercise.ToLower(),
            BatchNumber = m.BatchNumber,
            RougeType = m.RougeType,
            Precision = m.RougePrecision ?? 0,
            Recall = m.RougeRecall ?? 0,
            F1 = m.RougeF1 ?? 0
        }).ToList();

        return metrics;
    }

    public async Task<IResult> UpsertBertscoreBatchEvaluationAsync(UpsertBertScoreBatchEvaluationRequest request, CancellationToken cancellationToken)
    {
        var bertScoreMetrics = new BertScoreMetric
        {
            SessionId = request.SessionId,
            EvaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(request.EvaluationExercise),
            BatchNumber = request.BatchNumber,
            BertScorePrecision = request.Precision,
            BertScoreRecall = request.Recall,
            BertScoreF1 = request.F1
        };

        await _performanceMetricsDataService.UpsertChatCompletionPerformanceEvaluationAsync(bertScoreMetrics, cancellationToken);

        await _performanceMetricsDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<IResult> UpsertBleuBatchEvaluationAsync(UpsertBleuBatchEvaluationRequest request, CancellationToken cancellationToken)
    {
        var bleuMetric = new BleuMetric
        {
            SessionId = request.SessionId,
            EvaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(request.EvaluationExercise),
            BatchNumber = request.BatchNumber,
            BleuScore = request.BleuScore ?? 0,
        };

        await _performanceMetricsDataService.UpsertChatCompletionPerformanceEvaluationAsync(bleuMetric, cancellationToken);

        await _performanceMetricsDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<IResult> UpsertRougeScoreBatchEvaluationAsync(IEnumerable<UpsertRougeScoreBatchEvaluationRequest> requests, CancellationToken cancellationToken)
    {
        var rougeScoreMetrics = requests.Select(command => new RougeScoreMetric
        {
            SessionId = command.SessionId,
            EvaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(command.EvaluationExercise),
            BatchNumber = command.BatchNumber,
            RougeType = command.RougeType.ToLower(),
            RougePrecision = command.Precision,
            RougeRecall = command.Recall,
            RougeF1 = command.F1
        });

        await _performanceMetricsDataService.UpsertChatCompletionPerformanceEvaluationAsync(rougeScoreMetrics, cancellationToken);

        await _performanceMetricsDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
