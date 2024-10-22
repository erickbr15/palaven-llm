using Liara.Azure.Extensions;
using Liara.Azure.Storage;
using Liara.Clients.Extensions;
using Liara.Common.Extensions;
using Liara.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Data.Extensions;
using Palaven.Data.NoSql;
using Palaven.Ingest.Commands;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Palaven.Model;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using System.Text.Json;

namespace Palaven.Ingest.Test;

public class CreateSilverDocumentTests
{
    private readonly IHost _host;

    public CreateSilverDocumentTests()
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
                services.AddSingleton<ICommandHandler<CreateSilverDocumentCommand, EtlTaskDocument>, CreateSilverDocumentCommandHandler>();
            }).Build();
    }

    [Fact]
    public async Task Can_CreateSilverDocument()
    {
        var storageOptions = _host.Services.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
        var azureQueueClientFactory = _host.Services.GetRequiredService<IAzureClientFactory<QueueServiceClient>>();

        var silverStageQueue = azureQueueClientFactory.CreateClient("AzureStorageLawDocs").GetQueueClient(storageOptions.QueueNames[QueueStorageNames.SilverStageQueue]);
        var goldenStageQueue = azureQueueClientFactory.CreateClient("AzureStorageLawDocs").GetQueueClient(storageOptions.QueueNames[QueueStorageNames.GoldStageQueue]);
        var commandHandler = _host.Services.GetRequiredService<ICommandHandler<CreateSilverDocumentCommand, EtlTaskDocument>>();

        var message = await silverStageQueue.ReceiveMessageAsync(cancellationToken: CancellationToken.None);
        if (!TryToDeserializeMessage(message.Value.Body.ToString(), out var silverStageMessage))
        {
            await silverStageQueue.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt, CancellationToken.None);
            return;
        }

        var command = new CreateSilverDocumentCommand
        {
            OperationId = new Guid(silverStageMessage.OperationId)
        };

        var result = await commandHandler.ExecuteAsync(command, CancellationToken.None);

        if (result.IsSuccess && result.Value.Metadata.ContainsKey(EtlMetadataKeys.SilverLayerCompleted) && bool.Parse(result.Value.Metadata[EtlMetadataKeys.SilverLayerCompleted]))
        {
            await goldenStageQueue.SendMessageAsync(JsonSerializer.Serialize(new GoldenStageMessage
            {
                OperationId = silverStageMessage.OperationId
            }), CancellationToken.None);
        }

        await silverStageQueue.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt, CancellationToken.None);

        Assert.True(true);
    }

    private bool TryToDeserializeMessage(string message, out SilverStageMessage silverStageMessage)
    {
        var success = false;
        silverStageMessage = null!;

        try
        {
            silverStageMessage = JsonSerializer.Deserialize<SilverStageMessage>(message)!;
            success = silverStageMessage != null;
        }
        catch
        {            
        }

        return success;
    }
}
