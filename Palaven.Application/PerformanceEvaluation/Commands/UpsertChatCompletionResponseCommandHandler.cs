using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Application.PerformanceEvaluation.Commands;

public class UpsertChatCompletionResponseCommandHandler : ICommandHandler<UpsertChatCompletionResponseCommand>
{
    private readonly IEvaluationSessionDataService _evaluationSessionDataService;

    public UpsertChatCompletionResponseCommandHandler(IEvaluationSessionDataService performanceEvaluationDataService)
    {
        _evaluationSessionDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task<IResult> ExecuteAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            var result = Result.Fail(new ArgumentNullException(nameof(command)));
            return result;
        }

        var llmResponses = command.ChatCompletionResponses
            .Select(r => new LlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                EvaluationExerciseId = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeId(r.EvaluationExercise),
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value,
                ElapsedTime = r.ElapsedTime!.Value
            }).ToList();

        await _evaluationSessionDataService.UpsertChatCompletionResponseAsync(llmResponses, cancellationToken);

        await _evaluationSessionDataService.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
