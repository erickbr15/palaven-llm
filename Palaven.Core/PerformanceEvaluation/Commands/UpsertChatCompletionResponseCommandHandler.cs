using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Data.Entities;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class UpsertChatCompletionResponseCommandHandler : ICommandHandler<UpsertChatCompletionResponseCommand>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public UpsertChatCompletionResponseCommandHandler(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? 
            throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }        

    public async Task<IResult> ExecuteAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            var result = Result.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) });
            return result;
        }        

        var llmResponses = command.ChatCompletionResponses
            .Select(r => new LlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                EvaluationExerciseId = GetEvaluationExerciseId(r.EvaluationExercise),
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value,
                ElapsedTime = r.ElapsedTime!.Value
            }).ToList();

        await _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmResponses, cancellationToken);
        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static int GetEvaluationExerciseId(string chatCompletionEvaluationExcercise)
    {
        return chatCompletionEvaluationExcercise switch
        {
            ChatCompletionExcerciseType.LlmVanilla => 1,
            ChatCompletionExcerciseType.LlmWithRag => 2,
            ChatCompletionExcerciseType.LlmFineTuned => 3,
            ChatCompletionExcerciseType.LlmFineTunedAndRag => 4,
            _ => 0
        };
    }
}
