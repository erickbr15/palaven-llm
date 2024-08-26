using Liara.Common;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.Datasets;

public interface IFineTuningDatasetService
{
    Task CreateFineTuningPromptDatasetAsync(CreateFineTuningDataset model, CancellationToken cancellationToken);
    Task<IResult<List<FineTuningPromptData>>> FetchFineTuningPromptDatasetAsync(QueryFineTuningDataset model, CancellationToken cancellationToken);
}
