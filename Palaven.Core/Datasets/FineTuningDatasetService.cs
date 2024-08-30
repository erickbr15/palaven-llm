using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation.Commands;
using Palaven.Model.Datasets;
using System.Data;

namespace Palaven.Core.Datasets;

public class FineTuningDatasetService : IFineTuningDatasetService
{    
    private readonly IDatasetsDataService _datasetDataService;
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public FineTuningDatasetService(IDatasetsDataService instructionDataService, IPerformanceEvaluationDataService performanceEvaluationDataService)
    {        
        _datasetDataService = instructionDataService ?? throw new ArgumentNullException(nameof(instructionDataService));
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task CreateFineTuningPromptDatasetAsync(CreateFineTuningDataset model, CancellationToken cancellationToken)
    {
        var processedInstructions = _datasetDataService.GetFineTuningPromptQueryable()
            .Where(i => i.DatasetId == model.DatasetId)
            .Select(i => i.InstructionId)
            .ToList();

        var instructions = _datasetDataService.GetInstructionQueryable()
            .Where(i => !processedInstructions.Contains(i.Id))
            .ToList();

        const string Instruction_Mask = "{instruction}";
        const string Response_Mask = "{response}";

        foreach (var instruction in instructions)
        {
            var prompt = new FineTuningPromptEntity
            {
                InstructionId = instruction.Id,
                DatasetId = instruction.DatasetId,
                LargeLanguageModel = model.LargeLanguageModel,
                Prompt = Resources.GemmaPromptTemplates.FineTuningPrompt
                    .Replace(Instruction_Mask, instruction.Instruction)
                    .Replace(Response_Mask, instruction.Response)
            };

            await _datasetDataService.CreateAsync(prompt, cancellationToken);
        }
        await _datasetDataService.SaveChangesAsync(cancellationToken);
    }

    public async Task<IResult<List<FineTuningPromptData>?>> FetchFineTuningPromptDatasetAsync(QueryFineTuningDataset model, CancellationToken cancellationToken)
    {
        var evaluationSession = await _performanceEvaluationDataService.GetEvaluationSessionAsync(model.SessionId, cancellationToken);
        if (evaluationSession == null)
        {
            return Result<List<FineTuningPromptData>>.Success(new List<FineTuningPromptData>());
        }

        var offset = model.BatchNumber.HasValue ? (model.BatchNumber.Value - 1) * evaluationSession.BatchSize : 0;

        var instructionDataset = _datasetDataService.GetFineTuningPromptQueryable()
            .Where(x => x.DatasetId == evaluationSession.DatasetId)
            .OrderBy(x => x.PromptId)
            .Skip(offset)
            .Take(evaluationSession.BatchSize)
            .Select(i => new FineTuningPromptData
            {
                InstructionId = i.InstructionId,
                DatasetId = i.DatasetId,
                ChunckNumber = model.BatchNumber ?? 0,
                Prompt = i.Prompt,
                GoldenArticleId = i.Instruction.GoldenArticleId,
                LawId = i.Instruction.LawId,
                ArticleId = i.Instruction.ArticleId
            }).ToList();

        return Result<List<FineTuningPromptData>>.Success(instructionDataset);
    }   
}