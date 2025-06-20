﻿using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Application.PerformanceEvaluation.Commands;

public class CreateEvaluationSessionCommandHandler : ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo>
{
    private readonly IEvaluationSessionDataService _performanceEvaluationDataService;
    private readonly IDatasetsDataService _datasetDataService;

    public CreateEvaluationSessionCommandHandler(IEvaluationSessionDataService dataService, IDatasetsDataService datasetDataService)
    {
        _performanceEvaluationDataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _datasetDataService = datasetDataService ?? throw new ArgumentNullException(nameof(datasetDataService));
    }

    public async Task<IResult<EvaluationSessionInfo>> ExecuteAsync(CreateEvaluationSessionCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return Result<EvaluationSessionInfo>.Fail(new ArgumentNullException(nameof(command)));
        }

        var newEvaluationSession = BuildEvaluationSession(command);

        var evaluationInstructions = BuildEvaluationSessionInstructions(newEvaluationSession, command.TrainingAndValidationSplit);

        await _performanceEvaluationDataService.CreateEvaluationSessionAsync(newEvaluationSession, cancellationToken);

        await _performanceEvaluationDataService.AddInstructionToEvaluationSessionAsync(evaluationInstructions, cancellationToken);

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

    private List<EvaluationSessionInstruction> BuildEvaluationSessionInstructions(EvaluationSession evaluationSession, decimal trainingAndValidationSplit)
    {
        var instructionsToTrainAndValidateIds = new List<Guid>();
        var instructionsToTestIds = new List<Guid>();

        SplitInstructionsIntoTrainAndTestSets(instructionsToTrainAndValidateIds, instructionsToTestIds, evaluationSession.DatasetId, trainingAndValidationSplit);

        var trainingAndValidationInstructions = _datasetDataService.GetInstructionQueryable()
            .Where(x => instructionsToTrainAndValidateIds.Contains(x.InstructionId))
            .Select(x => new EvaluationSessionInstruction
            {
                EvaluationSessionId = evaluationSession.SessionId,
                InstructionId = x.InstructionId,
                InstructionPurpose = "train-validation"
            });

        var testInstructions = _datasetDataService.GetInstructionQueryable()
            .Where(x => instructionsToTestIds.Contains(x.InstructionId))
            .Select(x => new EvaluationSessionInstruction
            {
                EvaluationSessionId = evaluationSession.SessionId,
                InstructionId = x.InstructionId,
                InstructionPurpose = "test"
            });

        var evaluationSessionInstructions = new List<EvaluationSessionInstruction>();
        evaluationSessionInstructions.AddRange(trainingAndValidationInstructions);
        evaluationSessionInstructions.AddRange(testInstructions);

        return evaluationSessionInstructions;
    }

    private void SplitInstructionsIntoTrainAndTestSets(List<Guid> instructionsToTrainAndValidateIds, List<Guid> instructionsToTestIds, Guid datasetId, decimal trainingAndValidationSplit)
    {
        var instructions = _datasetDataService.GetInstructionQueryable().Where(x => x.DatasetId == datasetId).ToList();

        foreach (var articleInstructions in instructions.GroupBy(i => i.GoldenArticleId))
        {
            var qaInstructionIds = new Queue<Guid>(articleInstructions.Where(ai => ai.Category != "summarization").Select(ai => ai.InstructionId).ToList());

            var instructionsToTrainAndValidateCount = (int)Math.Round(qaInstructionIds.Count * trainingAndValidationSplit, 2, MidpointRounding.AwayFromZero);

            var instructionsToTestCount = qaInstructionIds.Count - instructionsToTrainAndValidateCount;

            while (true)
            {
                if (instructionsToTrainAndValidateCount == 0 && instructionsToTestCount == 0)
                {
                    break;
                }

                if (qaInstructionIds.Count == 0)
                {
                    break;
                }

                if (instructionsToTrainAndValidateCount > 0)
                {
                    instructionsToTrainAndValidateIds.Add(qaInstructionIds.Dequeue());
                    instructionsToTrainAndValidateCount--;
                }

                if (instructionsToTestCount > 0)
                {
                    instructionsToTestIds.Add(qaInstructionIds.Dequeue());
                    instructionsToTestCount--;
                }
            }

            var summarizationInstructionIds = articleInstructions.Where(ai => ai.Category == "summarization").Select(ai => ai.InstructionId).ToList();

            instructionsToTrainAndValidateIds.AddRange(summarizationInstructionIds);
            instructionsToTestIds.AddRange(summarizationInstructionIds);
        }
    }

    private static EvaluationSession BuildEvaluationSession(CreateEvaluationSessionCommand command)
    {
        return new EvaluationSession
        {
            SessionId = Guid.NewGuid(),
            DatasetId = command.DatasetId,
            BatchSize = command.BatchSize,
            LargeLanguageModel = command.LargeLanguageModel,
            DeviceInfo = command.DeviceInfo.ToLower(),
            IsActive = true,
            StartDate = DateTime.Now
        };
    }
}
