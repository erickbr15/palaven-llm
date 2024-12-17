using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Application.PerformanceEvaluation.Commands;

public class CleanChatCompletionResponseCommandHandler : ICommandHandler<CleanChatCompletionResponseCommand>
{
    private readonly IEvaluationSessionDataService _evaluationSessionDataService;

    public CleanChatCompletionResponseCommandHandler(IEvaluationSessionDataService performanceEvaluationDataService)
    {
        _evaluationSessionDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task<IResult> ExecuteAsync(CleanChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            var result = Result.Fail(new ArgumentNullException(nameof(command)));
            return result;
        }

        if (string.IsNullOrWhiteSpace(command.ChatCompletionExcerciseType))
        {
            var result = Result.Fail(new ArgumentException("The ChatCompletionExcerciseType is required"));
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
            x.EvaluationSessionId == command.SessionId &&
            x.BatchNumber == command.BatchNumber &&
            x.EvaluationExerciseId == evaluationExerciseId);

        _evaluationSessionDataService.CleanChatCompletionResponses(selectionCriteria, cleaningStrategy);

        await _evaluationSessionDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
