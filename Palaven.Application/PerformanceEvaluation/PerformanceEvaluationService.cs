using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Model.Persistence.Views;

namespace Palaven.Application.PerformanceEvaluation;

public class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo> _createEvaluationSessionCommand;
    private readonly IEvaluationSessionDataService _evaluationSessionDataService;    
    private readonly ICommandHandler<UpsertChatCompletionResponseCommand> _upsertChatCompletionResponseCommand;
    private readonly ICommandHandler<CleanChatCompletionResponseCommand> _cleanChatCompletionResponsesCommand;
    
    private readonly IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>> _queryChatCompletionResponsesCommand;

    public PerformanceEvaluationService(IEvaluationSessionDataService evaluationSessionDataService, ICommandHandler<UpsertChatCompletionResponseCommand> upsertChatCompletionResponseCommand,
        ICommandHandler<CleanChatCompletionResponseCommand> cleanChatCompletionResponsesCommand,
        ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo> createEvaluationSessionCommand,
        IQueryHandler<LlmChatCompletionResponseQuery, IList<LlmResponseView>> queryChatCompletionResponsesCommand)
    {
        _evaluationSessionDataService = evaluationSessionDataService ?? throw new ArgumentNullException(nameof(evaluationSessionDataService));
        _upsertChatCompletionResponseCommand = upsertChatCompletionResponseCommand ?? throw new ArgumentNullException(nameof(upsertChatCompletionResponseCommand));
        _cleanChatCompletionResponsesCommand = cleanChatCompletionResponsesCommand ?? throw new ArgumentNullException(nameof(cleanChatCompletionResponsesCommand));
        _createEvaluationSessionCommand = createEvaluationSessionCommand ?? throw new ArgumentNullException(nameof(createEvaluationSessionCommand));
        _queryChatCompletionResponsesCommand = queryChatCompletionResponsesCommand ?? throw new ArgumentNullException(nameof(queryChatCompletionResponsesCommand));
    }

    public Task<IResult<EvaluationSessionInfo>> CreateEvaluationSessionAsync(CreateEvaluationSessionCommand command, CancellationToken cancellationToken)
    {
        return _createEvaluationSessionCommand.ExecuteAsync(command, cancellationToken);
    }

    public async Task<EvaluationSessionInfo?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var evaluationSession = await _evaluationSessionDataService.GetEvaluationSessionAsync(sessionId, cancellationToken);

        if (evaluationSession == null)
        {
            return null;
        }

        return new EvaluationSessionInfo
        {
            SessionId = evaluationSession.SessionId,
            DatasetId = evaluationSession.DatasetId,
            BatchSize = evaluationSession.BatchSize,
            LargeLanguageModel = evaluationSession.LargeLanguageModel,
            DeviceInfo = evaluationSession.DeviceInfo,
            IsActive = evaluationSession.IsActive,
            StartDate = evaluationSession.StartDate
        };
    }

    public EvaluationSessionInfo? GetActiveEvaluationSessionByDataset(Guid datasetId)
    {
        var evaluationSession = _evaluationSessionDataService
            .GetEvaluationSessionQuery(x => x.DatasetId == datasetId && x.IsActive)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefault();

        if (evaluationSession == null)
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

    public Task<IResult> UpsertChatCompletionResponseAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        return _upsertChatCompletionResponseCommand.ExecuteAsync(command, cancellationToken);
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

        var result = _evaluationSessionDataService.FetchChatCompletionLlmInstructions(evaluationSessionId, evaluationExerciseId, batchNumber);

        var instructions = result.Select(i => new InstructionData
        {
            InstructionId = i.InstructionId,
            DatasetId = i.DatasetId,
            ChunckNumber = batchNumber,
            Instruction = i.Instruction,
            Response = i.Response,
            Category = i.Category,
            GoldenArticleId = i.GoldenArticleId,            
        }).ToList();

        return instructions;
    }

   
}
