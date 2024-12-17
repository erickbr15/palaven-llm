using Liara.Common;
using Liara.Common.Abstractions;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Model.Datasets;
using System.Data;

namespace Palaven.Application.DatasetManagement;

public class InstructionDatasetService : IInstructionDatasetService
{
    private readonly IDatasetsDataService _instructionDataService;
    private readonly IEvaluationSessionDataService _performanceEvaluationDataService;

    public InstructionDatasetService(IDatasetsDataService instructionDataService, IEvaluationSessionDataService performanceEvaluationDataService)
    {
        _instructionDataService = instructionDataService ?? throw new ArgumentNullException(nameof(instructionDataService));
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }
    
    public async Task<IResult<List<InstructionData>>> FetchInstructionsDatasetAsync(FetchInstructionsDatasetRequest model, CancellationToken cancellationToken)
    {
        var evaluationSession = await _performanceEvaluationDataService.GetEvaluationSessionAsync(model.SessionId, cancellationToken);
        if (evaluationSession == null)
        {
            return Result<List<InstructionData>>.Success(new List<InstructionData>());
        }

        var testInstructions = _instructionDataService.GetInstructionForTestingByEvaluationSession(model.SessionId, evaluationSession.BatchSize, model.BatchNumber);

        var instructionDataset = testInstructions
            .Select(i => new InstructionData
            {
                InstructionId = i.InstructionId,
                DatasetId = i.DatasetId,
                ChunckNumber = model.BatchNumber,
                Instruction = i.Instruction,
                Response = i.Response,
                Category = i.Category,
                GoldenArticleId = i.GoldenArticleId
            }).ToList();

        return Result<List<InstructionData>>.Success(instructionDataset);
    }
}