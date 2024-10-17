using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Liara.Azure.Extensions;
using Liara.Azure.Storage;
using Liara.Clients.Extensions;
using Liara.Common;
using Liara.Common.Extensions;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Palaven.Data.Extensions;
using Palaven.Data.NoSql;
using Palaven.Ingest.Commands;
using Palaven.Model;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;
using System.Text.Json;

namespace Palaven.Ingest.Test;

public class CompleteBronzeDocumentCommandHandlerTests
{
    private readonly IHost _host;

    public CompleteBronzeDocumentCommandHandlerTests()
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
                services.AddSingleton<ICommandHandler<CompleteBronzeDocumentCommand, EtlTaskDocument>, CompleteBronzeDocumentCommandHandler>();
            }).Build();
    }

    [Fact]
    public async Task CompleteBronzeDocumentCommandHandler_Run_WithNoErrors()
    {
        var storageOptionService = _host.Services.GetRequiredService<IOptions<AzureStorageOptions>>();
        var azureClientFactory = _host.Services.GetRequiredService<IAzureClientFactory<QueueServiceClient>>();
        var commandHandler = _host.Services.GetRequiredService<ICommandHandler<CompleteBronzeDocumentCommand, EtlTaskDocument>>();

        var storageOptions = storageOptionService?.Value ?? throw new ArgumentNullException(nameof(storageOptionService));

        var queueServiceClient = azureClientFactory.CreateClient("AzureStorageLawDocs") ??
            throw new InvalidOperationException($"Unable to create the queue client for AzureStorageLawDocs");

        var bronzeStageQueue = queueServiceClient.GetQueueClient(storageOptions.QueueNames[QueueStorageNames.BronzeStageQueue]);
        var silverStageQueue = queueServiceClient.GetQueueClient(storageOptions.QueueNames[QueueStorageNames.SilverStageQueue]);

        var azureResponse = await bronzeStageQueue.ReceiveMessageAsync(cancellationToken: CancellationToken.None);
        if (!TryToDeserializeMessage(azureResponse.Value.Body.ToString(), out var analysisMessage))
        {
            await bronzeStageQueue.DeleteMessageAsync(azureResponse.Value.MessageId, azureResponse.Value.PopReceipt, CancellationToken.None);
            return;
        }

        var command = new CompleteBronzeDocumentCommand
        {
            OperationId = new Guid(analysisMessage.OperationId),
            DocumentAnalysisOperationId = analysisMessage.DocumentAnalysisOperationId
        };

        var result = await commandHandler.ExecuteAsync(command, CancellationToken.None);

        await EnqueueSilverStageMessageAsync(silverStageQueue, result, CancellationToken.None);
        await UpdateBronzeStageQueueStateAsync(bronzeStageQueue, azureResponse.Value, result, CancellationToken.None);

        Assert.NotNull(result);
    }

    private bool TryToDeserializeMessage(string message, out BronzeStageMessage bronzeStageMessage)
    {
        var success = false;
        bronzeStageMessage = null!;

        try
        {
            bronzeStageMessage = JsonSerializer.Deserialize<BronzeStageMessage>(message)!;
            success = bronzeStageMessage != null;
        }
        catch {}

        return success;
    }

    private async Task UpdateBronzeStageQueueStateAsync(QueueClient _bronzeStageQueue, QueueMessage bronzeStageMessage, IResult<EtlTaskDocument> bronzeStageProcessResult, CancellationToken cancellationToken)
    {
        if (bronzeStageProcessResult.IsSuccess &&
            !bronzeStageProcessResult.Value.Metadata.ContainsKey(EtlMetadataKeys.BronzeLayerProcessed))
        {
            return;
        }
        await _bronzeStageQueue.DeleteMessageAsync(bronzeStageMessage.MessageId, bronzeStageMessage.PopReceipt, cancellationToken);
    }

    private async Task EnqueueSilverStageMessageAsync(QueueClient _silverStageQueue, IResult<EtlTaskDocument> bronzeStageProcessResult, CancellationToken cancellationToken)
    {
        if (bronzeStageProcessResult.IsSuccess && bool.Parse(bronzeStageProcessResult.Value.Metadata[EtlMetadataKeys.BronzeLayerCompleted]))
        {
            var silverStageMessage = new SilverStageMessage
            {
                OperationId = bronzeStageProcessResult.Value.Id
            };

            var message = JsonSerializer.Serialize(silverStageMessage);
            await _silverStageQueue.SendMessageAsync(message, cancellationToken: cancellationToken);
        }
    }
}
