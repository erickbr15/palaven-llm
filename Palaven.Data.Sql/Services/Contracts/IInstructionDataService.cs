using Palaven.Model.Datasets;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IInstructionDataService
{
    Task<InstructionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<InstructionEntity> CreateAsync(InstructionEntity instruction, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    IList<InstructionEntity> GetAll();
    IQueryable<InstructionEntity> GetQueryable();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<InstructionEntity> UpdateAsync(int id, InstructionEntity instruction, CancellationToken cancellationToken);
}
