using Liara.Azure.Extensions;
using Liara.Azure.Storage;
using Liara.Clients.Extensions;
using Liara.Common.Extensions;
using Liara.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Data.NoSql;
using Palaven.Data.Extensions;
using Palaven.Ingest.Commands;
using Palaven.Model.Ingest;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Liara.CosmosDb;
using Palaven.Model.Data.Documents;
using Microsoft.Azure.Cosmos;

namespace Palaven.Ingest.Test;

public class EtlProcessTests
{
    private readonly IHost _host;

    public EtlProcessTests()
    {
        _host = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, configBuilder) =>
            {
                var config = configBuilder.AddJsonFile("appsettings.json", optional: false).Build();
                var appConfigurationEndpoint = config.GetConnectionString("AppConfigurationEndpoint");

                configBuilder.AddAzureAppConfiguration(options =>
                {
                    var azureCredentialOptions = new DefaultAzureCredentialOptions();
                    var credentials = new DefaultAzureCredential(azureCredentialOptions);

                    options.Connect(new Uri(appConfigurationEndpoint!), credentials).Select(KeyFilter.Any);
                    options.ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(new DefaultAzureCredential());
                    });
                });
            })
            .ConfigureServices((hostContext, services) =>
            {
                var palavenCosmosOptions = new PalavenCosmosOptions();
                hostContext.Configuration.Bind("CosmosDB", palavenCosmosOptions);

                services.AddOptions<AzureStorageOptions>().BindConfiguration("AzureStorage");

                services.AddLiaraCommonServices();
                services.AddLiaraAzureServices(hostContext.Configuration);
                services.AddLiaraOpenAIServices();
                services.AddLiaraPineconeServices();
                services.AddNoSqlDataServices(palavenCosmosOptions);
                services.AddSingleton<ICommandHandler<StartBronzeDocumentCommand, string>, StartBronzeDocumentCommandHandler>();
            }).Build();
    }

    [Fact]
    public async Task CleanupTestResources()
    {
        var bronzeDocumentRepository = _host.Services.GetRequiredService<IDocumentRepository<BronzeDocument>>();

        var bronzeDocuments = await bronzeDocumentRepository.GetAsync(new QueryDefinition("SELECT * FROM c"),
            continuationToken: null,
            queryRequestOptions: null,
            CancellationToken.None);

        foreach (var bronzeDocument in bronzeDocuments)
        {
            await bronzeDocumentRepository.DeleteAsync(bronzeDocument.Id.ToString(), new PartitionKey(bronzeDocument.TenantId.ToString()), null, CancellationToken.None);
        }
    }
}