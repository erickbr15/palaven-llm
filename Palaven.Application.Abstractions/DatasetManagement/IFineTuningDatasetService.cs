using Liara.Common.Abstractions;
using Palaven.Application.Model.DatasetManagement;

namespace Palaven.Application.Abstractions.DatasetManagement;

public interface IFineTuningDatasetService
{
    Task CreateFineTuningPromptDatasetAsync(CreateFineTuningDatasetRequest request, CancellationToken cancellationToken);
    IResult<List<FineTuningPromptData>> FetchFineTuningPromptDataset(QueryFineTuningDatasetRequest request);
}
