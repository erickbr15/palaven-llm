using Liara.Integrations.Azure;
using Liara.Persistence.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Persistence.CosmosDB.Extensions;

public static class ApplicationRootExtensions
{
    public static void AddNoSqlDataServices(this IServiceCollection services,
        string connectionString,
        CosmosClientOptions? clientOptions,
        Dictionary<string, CosmosDBContainerOptions>? containerOptions)
    {

        services.AddAzureClients(azureClientBuilder =>
        {
            azureClientBuilder.AddClient<CosmosClient, CosmosClientOptions>(options => new CosmosClient(connectionString, clientOptions));
        });

        if(containerOptions != null)
        {
            services.AddTransient<IDocumentRepository<EtlTaskDocument>>(provider =>
            {

                var containerId = typeof(EtlTaskDocument).Name;
                var options = containerOptions.GetValueOrDefault(containerId);
                var client = provider.GetRequiredService<CosmosClient>();

                return new EtlTaskDocumentRepository(client, options!);
            });

            services.AddTransient<IDocumentRepository<BronzeDocument>>(provider =>
            {

                var containerId = typeof(BronzeDocument).Name;
                var options = containerOptions[containerId];
                var client = provider.GetRequiredService<CosmosClient>();

                return new BronzeDocumentRepository(client, options!);
            });

            services.AddTransient<IDocumentRepository<SilverDocument>>(provider =>
            {

                var containerId = typeof(SilverDocument).Name;
                var options = containerOptions[containerId];
                var client = provider.GetRequiredService<CosmosClient>();

                return new SilverDocumentRepository(client, options!);

            });

            services.AddTransient<IDocumentRepository<GoldenDocument>>(provider =>
            {

                var containerId = typeof(GoldenDocument).Name;
                var options = containerOptions[containerId];
                var client = provider.GetRequiredService<CosmosClient>();

                return new GoldenDocumentRepository(client, options!);
            });

            services.AddTransient<IDocumentRepository<DatasetGenerationTaskDocument>>(provider =>
            {

                var containerId = typeof(DatasetGenerationTaskDocument).Name;
                var options = containerOptions[containerId];
                var client = provider.GetRequiredService<CosmosClient>();

                return new DatasetGenerationTaskDocumentRepository(client, options!);
            });
        }        
    }
}
