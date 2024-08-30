using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Entities;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Core.PerformanceEvaluation;

public class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;
    private readonly ICommandHandler<UpsertChatCompletionResponseCommand> _upsertChatCompletionResponseCommand;
    private readonly ICommandHandler<CleanChatCompletionResponseCommand> _cleanChatCompletionResponsesCommand;
    private readonly IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>> _queryChatCompletionResponsesCommand;

    public PerformanceEvaluationService(IPerformanceEvaluationDataService performanceEvaluationDataService,
        ICommandHandler<UpsertChatCompletionResponseCommand> upsertChatCompletionResponseCommand,
        ICommandHandler<CleanChatCompletionResponseCommand> cleanChatCompletionResponsesCommand,
        IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>> queryChatCompletionResponsesCommand)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
        _upsertChatCompletionResponseCommand = upsertChatCompletionResponseCommand ?? throw new ArgumentNullException(nameof(upsertChatCompletionResponseCommand));
        _cleanChatCompletionResponsesCommand = cleanChatCompletionResponsesCommand ?? throw new ArgumentNullException(nameof(cleanChatCompletionResponsesCommand));
        _queryChatCompletionResponsesCommand = queryChatCompletionResponsesCommand ?? throw new ArgumentNullException(nameof(queryChatCompletionResponsesCommand));
    }

    public async Task<IResult<EvaluationSessionInfo?>> CreateEvaluationSessionAsync(CreateEvaluationSessionModel model, CancellationToken cancellationToken)
    {
        var evaluationSession = new EvaluationSession
        {
            DatasetId = model.DatasetId,
            BatchSize = model.BatchSize,
            LargeLanguageModel = model.LargeLanguageModel,
            DeviceInfo = model.DeviceInfo.ToLower(),
            IsActive = true,
            StartDate = DateTime.Now
        };

        var newEvaluationSession = await _performanceEvaluationDataService.CreateEvaluationSessionAsync(evaluationSession, cancellationToken);

        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);
        
        return Result<EvaluationSessionInfo?>.Success(new EvaluationSessionInfo
        {
            SessionId = newEvaluationSession.SessionId,
            DatasetId = newEvaluationSession.DatasetId,
            BatchSize = newEvaluationSession.BatchSize,
            LargeLanguageModel = newEvaluationSession.LargeLanguageModel,
            DeviceInfo = newEvaluationSession.DeviceInfo,
            IsActive = newEvaluationSession.IsActive,
            StartDate = newEvaluationSession.StartDate
        });
    }

    public async Task<EvaluationSessionInfo?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var evaluationSession = await _performanceEvaluationDataService.GetEvaluationSessionAsync(sessionId, cancellationToken);
        
        if(evaluationSession == null)
        {
            return null;
        }

        return new EvaluationSessionInfo
        {
            SessionId = evaluationSession.SessionId,
            DatasetId = evaluationSession.DatasetId,
            BatchSize = evaluationSession.BatchSize,
            LargeLanguageModel = evaluationSession.LargeLanguageModel,
            IsActive = evaluationSession.IsActive,
            StartDate = evaluationSession.StartDate
        };
    }

    public async Task<IResult> UpsertChatCompletionPerformanceEvaluationAsync(UpsertChatCompletionPerformanceEvaluation model, CancellationToken cancellationToken)
    {
        var bertScoreMetrics = new BertScoreMetric
        {
            SessionId = model.SessionId,
            BatchNumber = model.BatchNumber,
            BertScorePrecision = model.BertScorePrecision,
            BertScoreRecall = model.BertScoreRecall,
            BertScoreF1 = model.BertScoreF1
        };

        var rougeMetrics = model.RougeScoreMetrics.Select(x => new RougeScoreMetric
        {
            SessionId = model.SessionId,
            BatchNumber = model.BatchNumber,
            RougeType = x.RougeType,
            RougePrecision = x.RougeScorePrecision,
            RougeRecall = x.RougeScoreRecall,
            RougeF1 = x.RougeScoreF1
        });
        
        await _performanceEvaluationDataService.UpsertChatCompletionPerformanceEvaluationAsync(bertScoreMetrics, cancellationToken);
        await _performanceEvaluationDataService.UpsertChatCompletionPerformanceEvaluationAsync(rougeMetrics, cancellationToken);
        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<IResult> UpsertChatCompletionResponseAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        var result = await _upsertChatCompletionResponseCommand.ExecuteAsync(command, cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Fail(result.ValidationErrors, result.Exceptions);
    }

    public async Task<IResult> CleanChatCompletionResponseAsync(CleanChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        var result = await _cleanChatCompletionResponsesCommand.ExecuteAsync(command, cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Fail(result.ValidationErrors, result.Exceptions);        
    }

    public IList<LlmResponseView> FetchChatCompletionLlmResponses(Guid evaluationSessionId, int batchNumber, string chatCompletionExcerciseType)
    {
        var criteria = new LlmChatCompletionResponseQuery
        {
            ChatCompletionExcerciseType = chatCompletionExcerciseType,
            SelectionCriteria = x => x.EvaluationSessionId == evaluationSessionId && x.BatchNumber == batchNumber
        };

        var result = _queryChatCompletionResponsesCommand.ExecuteAsync(criteria, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return result.IsSuccess ? result.Value! : new List<LlmResponseView>();
    }
}
