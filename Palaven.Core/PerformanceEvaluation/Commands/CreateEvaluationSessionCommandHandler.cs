using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Data.Entities;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class CreateEvaluationSessionCommandHandler : ICommandHandler<CreateEvaluationSessionCommand, EvaluationSessionInfo>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;
    private readonly IDatasetsDataService _datasetDataService;

    public CreateEvaluationSessionCommandHandler(IPerformanceEvaluationDataService dataService, IDatasetsDataService datasetDataService)
    {
        _performanceEvaluationDataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _datasetDataService = datasetDataService ?? throw new ArgumentNullException(nameof(datasetDataService));
    }

    public async Task<IResult<EvaluationSessionInfo?>> ExecuteAsync(CreateEvaluationSessionCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return Result<EvaluationSessionInfo>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) });
        }                

        var newEvaluationSession = BuildEvaluationSession(command);
        var evaluationInstructions = BuildEvaluationSessionInstructions(newEvaluationSession, command.TrainingAndValidationSplit);

        var evaluationSession = await _performanceEvaluationDataService.CreateEvaluationSessionAsync(newEvaluationSession, cancellationToken);
        await _performanceEvaluationDataService.AddInstructionToEvaluationSessionAsync(evaluationInstructions, cancellationToken);

        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result<EvaluationSessionInfo?>.Success(new EvaluationSessionInfo
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
        var instructionsToTrainAndValidateIds = new List<int>();
        var instructionsToTestIds = new List<int>();

        SplitInstructionsIntoTrainAndTestSets(instructionsToTrainAndValidateIds, instructionsToTestIds, evaluationSession.DatasetId, trainingAndValidationSplit);
        
        var trainingAndValidationInstructions = _datasetDataService.GetInstructionQueryable()
            .Where(x => instructionsToTrainAndValidateIds.Contains(x.Id))
            .Select(x => new EvaluationSessionInstruction
            {
                EvaluationSessionId = evaluationSession.SessionId,
                InstructionId = x.Id,
                InstructionPurpose = "train-validation"                
            });

        var testInstructions = _datasetDataService.GetInstructionQueryable()
            .Where(x => instructionsToTestIds.Contains(x.Id))
            .Select(x => new EvaluationSessionInstruction
            {
                EvaluationSessionId = evaluationSession.SessionId,
                InstructionId = x.Id,
                InstructionPurpose = "test"
            });

        var evaluationSessionInstructions = new List<EvaluationSessionInstruction>();
        evaluationSessionInstructions.AddRange(trainingAndValidationInstructions);
        evaluationSessionInstructions.AddRange(testInstructions);

        return evaluationSessionInstructions;
    }

    private void SplitInstructionsIntoTrainAndTestSets(List<int> instructionsToTrainAndValidateIds, List<int> instructionsToTestIds, Guid datasetId, decimal trainingAndValidationSplit)
    {
        var instructions = _datasetDataService.GetInstructionQueryable().Where(x => x.DatasetId == datasetId).ToList();

        foreach (var articleInstructions in instructions.GroupBy(i => i.ArticleId))
        {
            var qaInstructionIds = new Queue<int>(articleInstructions.Where(ai => ai.Category != "summarization").Select(ai => ai.Id).ToList());

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

            var summarizationInstructionIds = articleInstructions.Where(ai => ai.Category == "summarization").Select(ai => ai.Id).ToList();

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
