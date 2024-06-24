using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation;

public class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public PerformanceEvaluationService(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task<IResult<EvaluationSessionInfo>> CreateEvaluationSessionAsync(CreateEvaluationSessionModel model, CancellationToken cancellationToken)
    {
        var evaluationSession = new EvaluationSession
        {
            DatasetId = model.DatasetId,
            BatchSize = model.BatchSize,
            LargeLanguageModel = model.LargeLanguageModel,
            IsActive = true,
            StartDate = DateTime.Now
        };

        var newEvaluationSession = await _performanceEvaluationDataService.CreateEvaluationSessionAsync(evaluationSession, cancellationToken);
        
        return Result<EvaluationSessionInfo>.Success(new EvaluationSessionInfo
        {
            SessionId = newEvaluationSession.SessionId,
            DatasetId = newEvaluationSession.DatasetId,
            BatchSize = newEvaluationSession.BatchSize,
            LargeLanguageModel = newEvaluationSession.LargeLanguageModel,
            IsActive = newEvaluationSession.IsActive,
            StartDate = newEvaluationSession.StartDate
        });
    }

    public async Task<IResult> UpsertChatCompletionPerformanceEvaluationAsync(UpsertChatCompletionPerformanceEvaluationModel model, CancellationToken cancellationToken)
    {
        var bertScoreMetrics = new BertScoreMetric
        {
            SessionId = model.SessionId,
            BatchNumber = model.BatchNumber,
            BertScorePrecision = model.BertScorePrecision,
            BertScoreRecall = model.BertScoreRecall,
            BertScoreF1 = model.BertScoreF1
        };
        
        await _performanceEvaluationDataService.UpsertChatCompletionPerformanceEvaluationAsync(bertScoreMetrics, cancellationToken);

        return Result.Success();
    }

    public Task<IResult> UpsertChatCompletionResponseAsync(IEnumerable<UpsertChatCompletionResponseModel> model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
