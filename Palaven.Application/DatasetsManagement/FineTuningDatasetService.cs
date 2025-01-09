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
    private readonly IPromptEngineeringService<string> _promptEngineeringService;

    public FineTuningDatasetService(IDatasetsDataService instructionDataService, IEvaluationSessionDataService evaluationSessionDataService,
        IPromptEngineeringService<string> promptEngineeringService)
    {
        _datasetDataService = instructionDataService ?? throw new ArgumentNullException(nameof(instructionDataService));
        _promptEngineeringService = promptEngineeringService ?? throw new ArgumentNullException(nameof(promptEngineeringService));
    }

    public async Task CreateFineTuningPromptDatasetAsync(CreateFineTuningDatasetRequest request, CancellationToken cancellationToken)
    {
        var processedInstructions = _datasetDataService.GetFineTuningPromptQueryable()
            .Select(i => i.InstructionId)
            .ToList();

        var instructions = _datasetDataService.GetInstructionQueryable()
            .Where(i => !processedInstructions.Contains(i.InstructionId))
            .ToList();
        

        foreach (var instruction in instructions)
        {
            var fineTuningPrompt = _promptEngineeringService.CreateFineTuningPrompt(instruction.Instruction, instruction.Response);

            if (!string.IsNullOrWhiteSpace(fineTuningPrompt))
            {
                var prompt = new FineTuningPromptEntity
                {
                    InstructionId = instruction.InstructionId,
                    LargeLanguageModel = request.LargeLanguageModel,
                    Prompt = fineTuningPrompt
                };

                await _datasetDataService.CreateAsync(prompt, cancellationToken);
            }            
        }
        await _datasetDataService.SaveChangesAsync(cancellationToken);
    }

    public IResult<List<FineTuningPromptData>> FetchFineTuningPromptDataset(QueryFineTuningDatasetRequest request)
    {                
        var instructionDataset = _datasetDataService.GetFineTuningPromptQueryable()
            .Where(p=>
                p.Instruction.DatasetId == request.DatasetId &&
                p.LargeLanguageModel.ToLower() == request.LargeLanguageModel.ToLower())
            .Select(i => new FineTuningPromptData
            {
                PromptId = i.PromptId,
                InstructionId = i.InstructionId,
                DatasetId = request.DatasetId,
                Prompt = i.Prompt,
                GoldenArticleId = i.Instruction.GoldenArticleId
            }).ToList();

        return Result<List<FineTuningPromptData>>.Success(instructionDataset);
    }
}