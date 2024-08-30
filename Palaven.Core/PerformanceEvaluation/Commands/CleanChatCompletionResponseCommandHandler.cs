using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class CleanChatCompletionResponseCommandHandler : ICommandHandler<CleanChatCompletionResponseCommand>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public CleanChatCompletionResponseCommandHandler(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? 
            throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task<IResult> ExecuteAsync(CleanChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.ChatCompletionExcerciseType))
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

        if (string.Equals(ChatCompletionExcerciseType.LlmVanilla, command.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<LlmResponse, bool>(x => x.SessionId == command.SessionId && x.BatchNumber == command.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if (string.Equals(ChatCompletionExcerciseType.LlmWithRag, command.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<LlmWithRagResponse, bool>(x => x.SessionId == command.SessionId && x.BatchNumber == command.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if (string.Equals(ChatCompletionExcerciseType.LlmFineTuned, command.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<FineTunedLlmResponse, bool>(x => x.SessionId == command.SessionId && x.BatchNumber == command.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }
        else if (string.Equals(ChatCompletionExcerciseType.LlmFineTunedAndRag, command.ChatCompletionExcerciseType, StringComparison.OrdinalIgnoreCase))
        {
            var selectionCriteria = new Func<FineTunedLlmWithRagResponse, bool>(x => x.SessionId == command.SessionId && x.BatchNumber == command.BatchNumber);
            _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        }

        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }    
}
