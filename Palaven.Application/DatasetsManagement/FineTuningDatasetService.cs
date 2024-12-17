using Liara.Common;
using Liara.Common.Abstractions;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Infrastructure.Abstractions.AI.Llm;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.Persistence.Entities;
using System.Data;

namespace Palaven.Application.DatasetManagement;

public class FineTuningDatasetService : IFineTuningDatasetService
{
    private readonly IDatasetsDataService _datasetDataService;
    private readonly IEvaluationSessionDataService _evaluationSessionDataService;
    private readonly IPromptEngineeringService<string> _promptEngineeringService;

    public FineTuningDatasetService(IDatasetsDataService instructionDataService, IEvaluationSessionDataService evaluationSessionDataService,
        IPromptEngineeringService<string> promptEngineeringService)
    {
        _datasetDataService = instructionDataService ?? throw new ArgumentNullException(nameof(instructionDataService));
        _evaluationSessionDataService = evaluationSessionDataService ?? throw new ArgumentNullException(nameof(evaluationSessionDataService));
        _promptEngineeringService = promptEngineeringService ?? throw new ArgumentNullException(nameof(promptEngineeringService));
    }

    public async Task CreateFineTuningPromptDatasetAsync(CreateFineTuningDatasetRequest request, CancellationToken cancellationToken)
    {
        var processedInstructions = _datasetDataService.GetFineTuningPromptQueryable()
            .Where(i => i.DatasetId == request.DatasetId)
            .Select(i => i.InstructionId)
            .ToList();

        var instructions = _datasetDataService.GetInstructionQueryable()
            .Where(i => !processedInstructions.Contains(i.InstructionId))
            .ToList();
        

        foreach (var instruction in instructions)
        {
            var fineTuningPrompt = _promptEngineeringService.CreateFineTuningPrompt(instruction.Instruction, instruction.Response);

            var prompt = new FineTuningPromptEntity
            {
                InstructionId = instruction.InstructionId,
                DatasetId = instruction.DatasetId,
                LargeLanguageModel = request.LargeLanguageModel,
                Prompt = fineTuningPrompt
            };

            await _datasetDataService.CreateAsync(prompt, cancellationToken);

        }
        await _datasetDataService.SaveChangesAsync(cancellationToken);
    }

    public async Task<IResult<List<FineTuningPromptData>>> FetchFineTuningPromptDatasetAsync(QueryFineTuningDatasetRequest request, CancellationToken cancellationToken)
    {
        var evaluationSession = await _evaluationSessionDataService.GetEvaluationSessionAsync(request.SessionId, cancellationToken);
        if (evaluationSession == null)
        {
            return Result<List<FineTuningPromptData>>.Success(new List<FineTuningPromptData>());
        }

        var offset = request.BatchNumber.HasValue ? (request.BatchNumber.Value - 1) * evaluationSession.BatchSize : 0;

        var instructionDataset = _datasetDataService.GetFineTuningPromptQueryable()
            .Where(x => x.DatasetId == evaluationSession.DatasetId)
            .OrderBy(x => x.PromptId)
            .Skip(offset)
            .Take(evaluationSession.BatchSize)
            .Select(i => new FineTuningPromptData
            {
                PromptId = i.PromptId,
                InstructionId = i.InstructionId,
                DatasetId = i.DatasetId,
                ChunckNumber = request.BatchNumber ?? 0,
                Prompt = i.Prompt,
                GoldenArticleId = i.Instruction.GoldenArticleId                
            }).ToList();

        return Result<List<FineTuningPromptData>>.Success(instructionDataset);
    }
}