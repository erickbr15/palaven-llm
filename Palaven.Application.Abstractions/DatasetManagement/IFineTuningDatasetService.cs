using Liara.Common.Abstractions;
using Palaven.Application.Model.DatasetManagement;

namespace Palaven.Application.Abstractions.DatasetManagement;

public interface IFineTuningDatasetService
{
    Task CreateFineTuningPromptDatasetAsync(CreateFineTuningDatasetRequest request, CancellationToken cancellationToken);
    Task<IResult<List<FineTuningPromptData>>> FetchFineTuningPromptDatasetAsync(QueryFineTuningDatasetRequest request, CancellationToken cancellationToken);
}
