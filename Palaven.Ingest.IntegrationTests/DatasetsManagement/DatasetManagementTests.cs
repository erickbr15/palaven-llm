using Azure.Identity;
using Liara.Common.Extensions;
using Liara.Integrations.Azure;
using Liara.Integrations.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Extensions;
using Palaven.Application.Notification.Extensions;
using Palaven.Data.Sql.Extensions;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.MicrosoftAzure.Extensions;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Persistence.CosmosDB.Extensions;

namespace Palaven.Ingest.IntegrationTests.DatasetsManagement;

public class DatasetManagementTests
{
    private readonly IHost _host;

    public DatasetManagementTests()
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
                services.AddLiaraCommonServices();
                services.AddLiaraOpenAIServices();
                services.AddLiaraPineconeServices();
                services.AddAzureAIServices(hostContext.Configuration);
                services.AddAzureStorageServices(hostContext.Configuration);

                var palavenDBConfig = hostContext.Configuration.GetSection("CosmosDB:Containers");
                var palavenDBConnectionString = hostContext.Configuration.GetConnectionString("PalavenCosmosDB");

                services.AddNoSqlDataServices(palavenDBConnectionString!, null, palavenDBConfig.Get<Dictionary<string, CosmosDBContainerOptions>>());
                
                var palavenSqlDBConnectionString = hostContext.Configuration.GetConnectionString("SqlDB");
                services.AddDataSqlServices(palavenSqlDBConnectionString!);
                services.AddNotificationService();
                services.AddDatasetManagementServices();
            }).Build();
    }

    [Fact]
    public async Task Can_CreateInstructionDataset_WithNoErrors()
    {
        var messageQueueService = _host.Services.GetRequiredService<IMessageQueueService>();
        var choreographyService = _host.Services.GetRequiredService<ICreateInstructionDatasetChoreographyService>();

        var message = await messageQueueService.ReceiveMessageAsync<CreateInstructionDatasetMessage>(cancellationToken: CancellationToken.None);
        var result = await choreographyService.CreateInstructionDatasetAsync(message, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.HasErrors);
    }
}
