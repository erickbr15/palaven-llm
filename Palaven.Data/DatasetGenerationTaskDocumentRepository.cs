using Liara.CosmosDb;
using Microsoft.Extensions.Options;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class DatasetGenerationTaskDocumentRepository : CosmosDbRepository<DatasetGenerationTaskDocument>
{
    public DatasetGenerationTaskDocumentRepository(IOptions<CosmosDbConnectionOptions> options)
        : base(options, "datasetgenerationtasks")
    {
    }
}
