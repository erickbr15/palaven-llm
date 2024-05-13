using Palaven.Model.Datasets;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IInstructionDataService
{
    Task<Instruction?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Instruction> CreateAsync(Instruction instruction, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    IList<Instruction> GetAll();
    IQueryable<Instruction> GetQueryable();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<Instruction> UpdateAsync(int id, Instruction instruction, CancellationToken cancellationToken);
}
