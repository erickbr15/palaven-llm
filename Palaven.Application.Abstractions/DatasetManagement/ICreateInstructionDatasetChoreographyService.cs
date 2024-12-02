using Liara.Common.Abstractions;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Abstractions.DatasetManagement;

public interface ICreateInstructionDatasetChoreographyService
{
    Task<IResult> CreateInstructionDatasetAsync(Message<CreateInstructionDatasetMessage> message, CancellationToken cancellationToken);
}
