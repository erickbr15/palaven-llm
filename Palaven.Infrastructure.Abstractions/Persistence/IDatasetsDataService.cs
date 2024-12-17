using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Infrastructure.Abstractions.Persistence;

public interface IDatasetsDataService
{
    Task<InstructionEntity?> GetInstructionByIdAsync(Guid id, CancellationToken cancellationToken);
    IEnumerable<InstructionEntity> GetInstructionForTestingByEvaluationSession(Guid sessionId, int sessionBatchSize, int? batchNumber);
    Task<FineTuningPromptEntity?> GetFineTuningPromptByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<InstructionEntity> CreateAsync(InstructionEntity instruction, CancellationToken cancellationToken);
    Task<FineTuningPromptEntity> CreateAsync(FineTuningPromptEntity prompt, CancellationToken cancellationToken);
    Task DeleteInstructionAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteFineTuningPromptAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsInstructionAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsFineTuningPromptAsync(Guid id, CancellationToken cancellationToken);
    IList<InstructionEntity> GetAllInstructions();
    IList<FineTuningPromptEntity> GetAllFineTuningPrompts();
    IQueryable<InstructionEntity> GetInstructionQueryable();
    IQueryable<FineTuningPromptEntity> GetFineTuningPromptQueryable();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<InstructionEntity> UpdateAsync(Guid id, InstructionEntity instruction, CancellationToken cancellationToken);
    Task<FineTuningPromptEntity> UpdateAsync(Guid id, FineTuningPromptEntity prompt, CancellationToken cancellationToken);
}
