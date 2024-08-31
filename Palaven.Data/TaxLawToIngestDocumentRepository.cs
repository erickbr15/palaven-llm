using Liara.CosmosDb;
using Microsoft.Extensions.Options;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class TaxLawToIngestDocumentRepository : CosmosDbRepository<TaxLawToIngestDocument>
{
    public TaxLawToIngestDocumentRepository(IOptions<CosmosDbConnectionOptions> options)
        : base(options, "taxlawdocs")
    {
    }
}
