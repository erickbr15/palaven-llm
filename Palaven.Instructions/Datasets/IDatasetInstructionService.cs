using Liara.Common;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.Datasets;

public interface IDatasetInstructionService
{
    Task CreateInstructionDatasetAsync(Guid traceId, Guid datasetId, CancellationToken cancellationToken);
    Task<IResult<List<InstructionData>>> FetchInstructionsDatasetAsync(QueryInstructionsDatasetModel model, CancellationToken cancellationToken);
}
