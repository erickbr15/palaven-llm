using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;
using System.Text;
using System.Text.RegularExpressions;

namespace Palaven.Core.PerformanceEvaluation;

public class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;
    private readonly ICommand<IEnumerable<UpsertChatCompletionResponseModel>, bool> _upsertChatCompletionResponseCommand;

    public PerformanceEvaluationService(IPerformanceEvaluationDataService performanceEvaluationDataService,
        ICommand<IEnumerable<UpsertChatCompletionResponseModel>, bool> upsertChatCompletionResponseCommand)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
        _upsertChatCompletionResponseCommand = upsertChatCompletionResponseCommand ?? throw new ArgumentNullException(nameof(upsertChatCompletionResponseCommand));
    }

    public async Task<IResult<EvaluationSessionInfo>> CreateEvaluationSessionAsync(CreateEvaluationSessionModel model, CancellationToken cancellationToken)
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
        
        return Result<EvaluationSessionInfo>.Success(new EvaluationSessionInfo
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
        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<IResult> UpsertChatCompletionResponseAsync(IEnumerable<UpsertChatCompletionResponseModel> model, CancellationToken cancellationToken)
    {
        var result = await _upsertChatCompletionResponseCommand.ExecuteAsync(model, cancellationToken);        

        return result.AnyErrorsOrValidationFailures ? 
            Result.Fail(result.ValidationErrors, result.Errors) : 
            Result.Success();
    }

    public async Task<IResult> CleanChatCompletionResponsesAsync(Guid sessionId, int batchNumber, string chatCompletionExcerciseType, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(chatCompletionExcerciseType))
        {
            throw new ArgumentNullException(nameof(chatCompletionExcerciseType));
        }

        var wordsToClean = new List<string> { "<eos>", "<eos>\"'", "**Answer:**", "**Respuesta:**" };
        
        var cleaningStrategy = new Func<string?, string>(x =>
        {
            var responseParts = x!.Split(new string[] { "<start_of_turn>model" }, StringSplitOptions.RemoveEmptyEntries);
            var cleanedResponse = responseParts[1].Trim();

            wordsToClean.ForEach(w =>
            {
                cleanedResponse = cleanedResponse.Replace(w, string.Empty);
            });

            return cleanedResponse;
        });

        if(string.Equals(ChatCompletionExcerciseType.LlmVanilla, chatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<LlmResponse, bool>(x => x.SessionId == sessionId && x.BatchNumber == batchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if(string.Equals(ChatCompletionExcerciseType.LlmWithRag, chatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<LlmWithRagResponse, bool>(x => x.SessionId == sessionId && x.BatchNumber == batchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if(string.Equals(ChatCompletionExcerciseType.LlmFineTuned, chatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<FineTunedLlmResponse, bool>(x => x.SessionId == sessionId && x.BatchNumber == batchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if(string.Equals(ChatCompletionExcerciseType.LlmFineTunedAndRag, chatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<FineTunedLlmWithRagResponse, bool>(x => x.SessionId == sessionId && x.BatchNumber == batchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }        

        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
