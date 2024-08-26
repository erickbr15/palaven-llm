using Palaven.Model.Datasets;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IDatasetsDataService
{
    Task<InstructionEntity?> GetInstructionByIdAsync(int id, CancellationToken cancellationToken);
    Task<FineTuningPromptEntity?> GetFineTuningPromptByIdAsync(int id, CancellationToken cancellationToken);
    Task<InstructionEntity> CreateAsync(InstructionEntity instruction, CancellationToken cancellationToken);
    Task<FineTuningPromptEntity> CreateAsync(FineTuningPromptEntity prompt, CancellationToken cancellationToken);
    Task DeleteInstructionAsync(int id, CancellationToken cancellationToken);
    Task DeleteFineTuningPromptAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsInstructionAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsFineTuningPromptAsync(int id, CancellationToken cancellationToken);
    IList<InstructionEntity> GetAllInstructions();
    IList<FineTuningPromptEntity> GetAllFineTuningPrompts();
    IQueryable<InstructionEntity> GetInstructionQueryable();
    IQueryable<FineTuningPromptEntity> GetFineTuningPromptQueryable();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<InstructionEntity> UpdateAsync(int id, InstructionEntity instruction, CancellationToken cancellationToken);
    Task<FineTuningPromptEntity> UpdateAsync(int id, FineTuningPromptEntity prompt, CancellationToken cancellationToken);
}
