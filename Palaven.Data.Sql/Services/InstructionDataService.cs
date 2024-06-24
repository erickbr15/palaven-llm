using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Datasets;

namespace Palaven.Data.Sql.Services;

public class InstructionDataService : IInstructionDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<InstructionEntity> _repository;

    public InstructionDataService(PalavenDbContext dbContext, IRepository<InstructionEntity> repository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<InstructionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<InstructionEntity> CreateAsync(InstructionEntity instruction, CancellationToken cancellationToken)
    {            
        await _repository.AddAsync(instruction, cancellationToken);
        return instruction;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var instruction = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (instruction != null)
        {
            _repository.Delete(instruction);
        }        
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return _repository.ExistsAsync(id, cancellationToken);
    }

    public IList<InstructionEntity> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public IQueryable<InstructionEntity> GetQueryable()
    {
        return _repository.GetAll();
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
        var entity = await _repository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException($"Unable to find the entity with id {id}.");        

        entity.Instruction = instruction.Instruction;
        entity.Response = instruction.Response;
        entity.Category = instruction.Category;
        entity.GoldenArticleId = instruction.GoldenArticleId;
        entity.LawId = instruction.LawId;
        entity.ArticleId = instruction.ArticleId;

        return entity;
    }
}
