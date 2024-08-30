using Liara.CosmosDb;
using Microsoft.Extensions.Options;
using Palaven.Model.Ingest.Documents;

namespace Palaven.Data;

public class TaxLawArticleDocumentRepository : CosmosDbRepository<TaxLawDocumentArticle>
{
    public TaxLawArticleDocumentRepository(IOptions<CosmosDbConnectionOptions> options)
        : base(options, "taxlawdocarticles")
    {
    }
}
