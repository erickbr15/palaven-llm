using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Identity;
using Liara.Common.Abstractions.Persistence;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Persistence.CosmosDB.Extensions;
using Liara.Integrations.Azure;

namespace Palaven.Ingest.Test.Ingest;

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
                var palavenDbConnectionString = hostContext.Configuration.GetConnectionString("PalavenCosmosDB");

                var containerOptions = new Dictionary<string, CosmosDBContainerOptions>();
                hostContext.Configuration.GetSection("CosmosDB:Containers").Bind(containerOptions);

                services.AddNoSqlDataServices(palavenDbConnectionString!, clientOptions: null, containerOptions);

            }).Build();
    }

    [Fact]
    public async Task CleanupTest_BronzeDocuments()
    {
        var repository = _host.Services.GetRequiredService<IDocumentRepository<BronzeDocument>>();
        var bronzeDocuments = await repository.GetAsync("SELECT * FROM c", continuationToken: null, CancellationToken.None);

        foreach (var bronzeDocument in bronzeDocuments)
        {
            await repository.DeleteAsync(bronzeDocument.Id.ToString(), bronzeDocument.TenantId.ToString(), CancellationToken.None);
        }

        Assert.True(true);
    }

    [Fact]
    public async Task CleanupTest_SilverDocuments()
    {
        var repository = _host.Services.GetRequiredService<IDocumentRepository<SilverDocument>>();
        var silverDocuments = await repository.GetAsync("SELECT * FROM c", continuationToken: null, CancellationToken.None);

        foreach (var silverDocument in silverDocuments)
        {
            await repository.DeleteAsync(silverDocument.Id.ToString(), silverDocument.TenantId.ToString(), CancellationToken.None);
        }

        Assert.True(true);
    }

    [Fact]
    public async Task CleanupTest_GoldenDocuments()
    {
        var repository = _host.Services.GetRequiredService<IDocumentRepository<GoldenDocument>>();
        var goldenDocuments = await repository.GetAsync("SELECT * FROM c", continuationToken: null, CancellationToken.None);

        foreach (var goldenDocument in goldenDocuments)
        {
            await repository.DeleteAsync(goldenDocument.Id.ToString(), goldenDocument.TenantId.ToString(), CancellationToken.None);
        }

        Assert.True(true);
    }
}