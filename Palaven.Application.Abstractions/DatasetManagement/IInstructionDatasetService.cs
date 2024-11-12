using Liara.Common.Abstractions;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Model.Datasets;

namespace Palaven.Application.Abstractions.DatasetManagement;

public interface IInstructionDatasetService
{
    Task<IResult<List<InstructionData>>> FetchInstructionsDatasetAsync(FetchInstructionsDatasetRequest model, CancellationToken cancellationToken);
}
