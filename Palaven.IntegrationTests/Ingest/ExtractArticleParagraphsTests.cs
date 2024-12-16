using Azure.Identity;
using Liara.Common.Extensions;
using Liara.Integrations.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Palaven.Infrastructure.MicrosoftAzure.Extensions;
using Palaven.Application.Ingest.Extensions;
using Palaven.Persistence.CosmosDB.Extensions;
using Palaven.Infrastructure.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Notification.Extensions;

namespace Palaven.Ingest.Test.Ingest;

public class ExtractArticleParagraphsTests
{
    private readonly IHost _host;

    public ExtractArticleParagraphsTests()
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
                services.AddIngestServices();
                services.AddNotificationService();
            }).Build();
    }

    [Fact]
    public async Task ExtractArticleParagraphs_Run_WithNoErrors()
    {
        var queueMessageService = _host.Services.GetRequiredService<IMessageQueueService>();

        var message = await queueMessageService.ReceiveMessageAsync<ExtractArticleParagraphsMessage>(cancellationToken: CancellationToken.None);

        var choreographyService = _host.Services.GetRequiredService<IArticleParagraphsExtractionChoreographyService>();

        var result = await choreographyService.ExtractArticleParagraphsAsync(message, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
}
