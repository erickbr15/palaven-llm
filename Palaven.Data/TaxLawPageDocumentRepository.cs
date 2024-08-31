using Liara.CosmosDb;
using Microsoft.Extensions.Options;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class TaxLawPageDocumentRepository : CosmosDbRepository<TaxLawDocumentPage>
{
    public TaxLawPageDocumentRepository(IOptions<CosmosDbConnectionOptions> options) 
        : base(options, "taxlawdocpages")
    {                
    }
}
