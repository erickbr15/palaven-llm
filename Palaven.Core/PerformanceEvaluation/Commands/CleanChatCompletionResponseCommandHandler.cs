using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Data.Entities;
using Palaven.Model.PerformanceEvaluation;

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
            var result = Result.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) });
            return result;
        }

        if (string.IsNullOrWhiteSpace(command.ChatCompletionExcerciseType))
        {
            var result = Result.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentException("The ChatCompletionExcerciseType is required") });
            return result;
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

        var evaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(command.ChatCompletionExcerciseType);
        
        var selectionCriteria = new Func<LlmResponse, bool>(x => 
            x.SessionId == command.SessionId && 
            x.BatchNumber == command.BatchNumber &&
            x.EvaluationExerciseId == evaluationExerciseId);

        _performanceEvaluationDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);
        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
