using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Model.Data.Documents;

namespace Palaven.Data.Extensions;

public static class ApplicationRootExtensions
{
    public static void AddDataServices(this IServiceCollection services, PalavenCosmosOptions cosmosOptions)
    {
        services.AddAzureClients(azureClientBuilder => {
            azureClientBuilder.AddClient<CosmosClient, CosmosClientOptions>(options => new CosmosClient(cosmosOptions.ConnectionString, options));
        });
                
        services.AddTransient<IDocumentRepository<TaxLawIngestTaskDocument>>(provider => {

            var containerId = typeof(TaxLawIngestTaskDocument).Name.ToLower();
            var containerOptions = cosmosOptions.ContainerOptions[containerId];
            var client = provider.GetRequiredService<CosmosClient>();

            return new TaxLawIngestTaskDocumentRepository(client, containerOptions);
        });

        services.AddTransient<IDocumentRepository<StartTaxLawIngestTaskDocument>>(provider => {

            var containerId = typeof(StartTaxLawIngestTaskDocument).Name.ToLower();
            var containerOptions = cosmosOptions.ContainerOptions[containerId];
            var client = provider.GetRequiredService<CosmosClient>();

            return new TaxLawToIngestDocumentRepository(client, containerOptions);
        });

        services.AddTransient<IDocumentRepository<DatasetGenerationTaskDocument>>(provider => {

            var containerId = typeof(DatasetGenerationTaskDocument).Name.ToLower();
            var containerOptions = cosmosOptions.ContainerOptions[containerId];
            var client = provider.GetRequiredService<CosmosClient>();

            return new DatasetGenerationTaskDocumentRepository(client, containerOptions);

        });

        services.AddTransient<IDocumentRepository<BronzeDocument>>(provider => {

            var containerId = typeof(BronzeDocument).Name.ToLower();
            var containerOptions = cosmosOptions.ContainerOptions[containerId];
            var client = provider.GetRequiredService<CosmosClient>();

            return new BronzeDocumentRepository(client, containerOptions);
        });

        services.AddTransient<IDocumentRepository<SilverDocument>>(provider => {
            
            var containerId = typeof(SilverDocument).Name.ToLower();
            var containerOptions = cosmosOptions.ContainerOptions[containerId];
            var client = provider.GetRequiredService<CosmosClient>();

            return new SilverDocumentRepository(client, containerOptions);

        });

        services.AddTransient<IDocumentRepository<GoldenDocument>>(provider => {

            var containerId = typeof(GoldenDocument).Name.ToLower();
            var containerOptions = cosmosOptions.ContainerOptions[containerId];
            var client = provider.GetRequiredService<CosmosClient>();

            return new GoldenDocumentRepository(client, containerOptions);
        });        
    }
}
