using Liara.Common;
using Palaven.Model.Datasets;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Core.Datasets;

public interface IInstructionDatasetService
{
    Task CreateInstructionDatasetAsync(CreateInstructionDataset model, CancellationToken cancellationToken);
    Task<IResult<List<InstructionData>?>> FetchInstructionsDatasetAsync(FetchInstructionsDataset model, CancellationToken cancellationToken);
}
