using Liara.Common;
using Microsoft.EntityFrameworkCore;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Entities;
using Palaven.Model.PerformanceEvaluation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Palaven.Core.PerformanceEvaluation;

public class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;
    private readonly IPerformanceMetricsDataService _performanceMetricsDataService;
    private readonly ICommandHandler<UpsertChatCompletionResponseCommand> _upsertChatCompletionResponseCommand;
    private readonly ICommandHandler<CleanChatCompletionResponseCommand> _cleanChatCompletionResponsesCommand;
    private readonly ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo> _createEvaluationSessionCommand;
    private readonly IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>> _queryChatCompletionResponsesCommand;

    public PerformanceEvaluationService(IPerformanceEvaluationDataService performanceEvaluationDataService,
        IPerformanceMetricsDataService performanceMetricsDataService,
        ICommandHandler<UpsertChatCompletionResponseCommand> upsertChatCompletionResponseCommand,
        ICommandHandler<CleanChatCompletionResponseCommand> cleanChatCompletionResponsesCommand,
        ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo> createEvaluationSessionCommand,
        IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>> queryChatCompletionResponsesCommand)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
        _performanceMetricsDataService = performanceMetricsDataService ?? throw new ArgumentNullException(nameof(performanceMetricsDataService));
        _upsertChatCompletionResponseCommand = upsertChatCompletionResponseCommand ?? throw new ArgumentNullException(nameof(upsertChatCompletionResponseCommand));
        _cleanChatCompletionResponsesCommand = cleanChatCompletionResponsesCommand ?? throw new ArgumentNullException(nameof(cleanChatCompletionResponsesCommand));
        _createEvaluationSessionCommand = createEvaluationSessionCommand ?? throw new ArgumentNullException(nameof(createEvaluationSessionCommand));
        _queryChatCompletionResponsesCommand = queryChatCompletionResponsesCommand ?? throw new ArgumentNullException(nameof(queryChatCompletionResponsesCommand));
    }

    public async Task<IResult<EvaluationSessionInfo?>> CreateEvaluationSessionAsync(CreateEvaluationSessionCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return Result<EvaluationSessionInfo>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) });
        }

        var result = await _createEvaluationSessionCommand.ExecuteAsync(command, cancellationToken);

        return result;
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

    public EvaluationSessionInfo? GetActiveEvaluationSessionByDataset(Guid datasetId)
    {
        var evaluationSession = _performanceEvaluationDataService
            .GetEvaluationSessionQuery(x => x.DatasetId == datasetId && x.IsActive)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefault();

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

    public async Task<IResult> UpsertChatCompletionPerformanceEvaluationAsync(UpsertBertscoreBatchEvaluationCommand command, CancellationToken cancellationToken)
    {
        var bertScoreMetrics = new BertScoreMetric
        {
            SessionId = command.SessionId,
            EvaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(command.EvaluationExercise),
            BatchNumber = command.BatchNumber,
            BertScorePrecision = command.BertScorePrecision,
            BertScoreRecall = command.BertScoreRecall,
            BertScoreF1 = command.BertScoreF1
        };        
        
        await _performanceMetricsDataService.UpsertChatCompletionPerformanceEvaluationAsync(bertScoreMetrics, cancellationToken);
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
            SelectionCriteria = x => x.EvaluationSessionId == evaluationSessionId && x.BatchNumber == batchNumber && x.EvaluationExercise == chatCompletionExcerciseType
        };

        var result = _queryChatCompletionResponsesCommand.ExecuteAsync(criteria, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return result.IsSuccess ? result.Value! : new List<LlmResponseView>();
    }

    public IList<InstructionData> FetchChatCompletionLlmInstructions(Guid evaluationSessionId, int batchNumber, string chatCompletionExcerciseType)
    {
        var evaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(chatCompletionExcerciseType);

        var result = _performanceEvaluationDataService.FetchChatCompletionLlmInstructions(evaluationSessionId, evaluationExerciseId, batchNumber);

        var instructions = result.Select(i => new InstructionData
        {
            InstructionId = i.Id,
            DatasetId = i.DatasetId,
            ChunckNumber = batchNumber,
            Instruction = i.Instruction,
            Response = i.Response,
            Category = i.Category,
            GoldenArticleId = i.GoldenArticleId,
            LawId = i.LawId,
            ArticleId = i.ArticleId
          }).ToList();

        return instructions;
    }
}
