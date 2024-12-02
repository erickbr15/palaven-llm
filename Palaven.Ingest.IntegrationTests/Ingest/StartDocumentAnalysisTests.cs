using Liara.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Application.Abstractions.Ingest;
using Liara.Integrations.Azure;
using Palaven.Infrastructure.MicrosoftAzure.Extensions;
using Palaven.Persistence.CosmosDB.Extensions;
using Palaven.Application.Ingest.Extensions;
using Palaven.Application.Notification.Extensions;

namespace Palaven.Ingest.Test.Ingest;

public class StartDocumentAnalysisTests
{
    private readonly IHost _host;

    public StartDocumentAnalysisTests()
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
                services.AddAzureAIServices(hostContext.Configuration);
                services.AddAzureStorageServices(hostContext.Configuration);

                var palavenDBConfig = hostContext.Configuration.GetSection("CosmosDB:Containers");
                var palavenDBConnectionString = hostContext.Configuration.GetConnectionString("PalavenCosmosDB");

                services.AddNoSqlDataServices(palavenDBConnectionString!, null, palavenDBConfig.Get<Dictionary<string, CosmosDBContainerOptions>>());
                services.AddNotificationService();
                services.AddIngestServices();
            }).Build();
    }

    [Fact]
    public async Task StartDocumentAnalysis_Run_WithNoErrors()
    {
        var messageQueueService = _host.Services.GetRequiredService<IMessageQueueService>();
        var coreographyService = _host.Services.GetRequiredService<IDocumentAnalysisChoreographyService>();

        var message = await messageQueueService.ReceiveMessageAsync<DocumentAnalysisMessage>(cancellationToken: CancellationToken.None);
        var result = await coreographyService.StartDocumentAnalysisAsync(message, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(!string.IsNullOrWhiteSpace(result.Value));
    }
}
