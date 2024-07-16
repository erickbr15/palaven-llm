using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class CleanChatCompletionResponsesCommand : ICommand<CleanChatCompletionResponsesModel, bool>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public CleanChatCompletionResponsesCommand(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task<IResult<bool>> ExecuteAsync(CleanChatCompletionResponsesModel inputModel, CancellationToken cancellationToken)
    {
        if(inputModel == null)
        {
            throw new ArgumentNullException(nameof(inputModel));
        }            
        if (string.IsNullOrWhiteSpace(inputModel.ChatCompletionExcerciseType))
        {
            throw new ArgumentException("The ChatCompletionExcerciseType is required");
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

            cleanedResponse = cleanedResponse.Trim();

            return cleanedResponse;
        });

        if (string.Equals(ChatCompletionExcerciseType.LlmVanilla, inputModel.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<LlmResponse, bool>(x => x.SessionId == inputModel.SessionId && x.BatchNumber == inputModel.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if (string.Equals(ChatCompletionExcerciseType.LlmWithRag, inputModel.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<LlmWithRagResponse, bool>(x => x.SessionId == inputModel.SessionId && x.BatchNumber == inputModel.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if (string.Equals(ChatCompletionExcerciseType.LlmFineTuned, inputModel.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<FineTunedLlmResponse, bool>(x => x.SessionId == inputModel.SessionId && x.BatchNumber == inputModel.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if (string.Equals(ChatCompletionExcerciseType.LlmFineTunedAndRag, inputModel.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<FineTunedLlmWithRagResponse, bool>(x => x.SessionId == inputModel.SessionId && x.BatchNumber == inputModel.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }

        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
