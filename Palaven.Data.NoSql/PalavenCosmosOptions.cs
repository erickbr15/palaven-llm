using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace Palaven.Data.NoSql;

public class PalavenCosmosOptions
{
    public string ConnectionString { get; set; } = default!;
    public CosmosClientOptions? ClientOptions { get; set; }
    public IDictionary<string, CosmosContainerOptions> ContainerOptions { get; set; } = new Dictionary<string, CosmosContainerOptions>();
}
