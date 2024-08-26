using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Datasets;

namespace Palaven.Data.Sql.Services;

public class DatasetsDataService : IDatasetsDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<InstructionEntity> _instructionRepository;
    private readonly IRepository<FineTuningPromptEntity> _fineTuningPromptRepository;

    public DatasetsDataService(PalavenDbContext dbContext, IRepository<InstructionEntity> instructionRepository,
        IRepository<FineTuningPromptEntity> fineTuningPromptRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _instructionRepository = instructionRepository ?? throw new ArgumentNullException(nameof(instructionRepository));
        _fineTuningPromptRepository = fineTuningPromptRepository ?? throw new ArgumentNullException(nameof(fineTuningPromptRepository));
    }

    public Task<InstructionEntity?> GetInstructionByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _instructionRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<InstructionEntity> CreateAsync(InstructionEntity instruction, CancellationToken cancellationToken)
    {            
        await _instructionRepository.AddAsync(instruction, cancellationToken);
        return instruction;
    }

    public async Task DeleteInstructionAsync(int id, CancellationToken cancellationToken)
    {
        var instruction = await _instructionRepository.GetByIdAsync(id, cancellationToken);
        
        if (instruction != null)
        {
            _instructionRepository.Delete(instruction);
        }        
    }

    public Task<bool> ExistsInstructionAsync(int id, CancellationToken cancellationToken)
    {
        return _instructionRepository.ExistsAsync(id, cancellationToken);
    }

    public IList<InstructionEntity> GetAllInstructions()
    {
        return _instructionRepository.GetAll().ToList();
    }

    public IQueryable<InstructionEntity> GetInstructionQueryable()
    {
        return _instructionRepository.GetAll();
    }

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<InstructionEntity> UpdateAsync(int id, InstructionEntity instruction, CancellationToken cancellationToken)
    {
        var entity = await _instructionRepository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException($"Unable to find the entity with id {id}.");        

        entity.Instruction = instruction.Instruction;
        entity.Response = instruction.Response;
        entity.Category = instruction.Category;
        entity.GoldenArticleId = instruction.GoldenArticleId;
        entity.LawId = instruction.LawId;
        entity.ArticleId = instruction.ArticleId;

        return entity;
    }

    public Task<FineTuningPromptEntity?> GetFineTuningPromptByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _fineTuningPromptRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<FineTuningPromptEntity> CreateAsync(FineTuningPromptEntity prompt, CancellationToken cancellationToken)
    {
        await _fineTuningPromptRepository.AddAsync(prompt, cancellationToken);
        return prompt;
    }

    public async Task DeleteFineTuningPromptAsync(int id, CancellationToken cancellationToken)
    {
        var prompt = await _fineTuningPromptRepository.GetByIdAsync(id, cancellationToken);
        if (prompt != null)
        {
            _fineTuningPromptRepository.Delete(prompt);
        }
    }    

    public Task<bool> ExistsFineTuningPromptAsync(int id, CancellationToken cancellationToken)
    {
        return _fineTuningPromptRepository.ExistsAsync(id, cancellationToken);
    }

    public IList<FineTuningPromptEntity> GetAllFineTuningPrompts()
    {
        return _fineTuningPromptRepository.GetAll().ToList();
    }

    public IQueryable<FineTuningPromptEntity> GetFineTuningPromptQueryable()
    {
        return _fineTuningPromptRepository.GetAll();
    }    

    public async Task<FineTuningPromptEntity> UpdateAsync(int id, FineTuningPromptEntity prompt, CancellationToken cancellationToken)
    {
        var entity = await _fineTuningPromptRepository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException($"Unable to find the entity with id {id}.");

        entity.Instruction = prompt.Instruction;
        entity.DatasetId = prompt.DatasetId;
        entity.LargeLanguageModel = prompt.LargeLanguageModel;
        entity.Prompt = prompt.Prompt;

        return entity;
    }
}
