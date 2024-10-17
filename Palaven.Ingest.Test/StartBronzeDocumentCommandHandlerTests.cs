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
using Palaven.Model.Ingest;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Palaven.Model;
using System.Text.Json;

namespace Palaven.Ingest.Test;

public class StartBronzeDocumentCommandHandlerTests
{
    private readonly IHost _host;

    public StartBronzeDocumentCommandHandlerTests()
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
    public async Task StartBronzeDocumentCommandHandler_Run_WithNoErrors()
    {                        
        var storageOptionsService = _host.Services.GetRequiredService<IOptions<AzureStorageOptions>>();
        var blobServiceClientFactory = _host.Services.GetRequiredService<IAzureClientFactory<BlobServiceClient>>();
        var queueServiceClientFactory = _host.Services.GetRequiredService<IAzureClientFactory<QueueServiceClient>>();

        var blobServiceClient = blobServiceClientFactory.CreateClient("AzureStorageLawDocs")
        ?? throw new InvalidOperationException("Unable to create BlobServiceClient");

        var queueClientService = queueServiceClientFactory.CreateClient("AzureStorageLawDocs")
            ?? throw new InvalidOperationException("Unable to create QueueServiceClient");
        
        var documentAnalysisClient = queueClientService.GetQueueClient(storageOptionsService.Value.QueueNames[QueueStorageNames.DocumentAnalysisQueue]);

        var sut = _host.Services.GetRequiredService<ICommandHandler<StartBronzeDocumentCommand, string>>();

        var azureResponse = await documentAnalysisClient.ReceiveMessageAsync(cancellationToken: default);
        var message = azureResponse.Value;

        if (!TryToDeserializeMessage(message.MessageText, out var documentAnalysisMessage))
        {
            Assert.Fail("Failed to deserialize message");
            return;
        }

        var container = blobServiceClient.GetBlobContainerClient(storageOptionsService.Value.BlobContainers[BlobStorageContainers.EtlInbox]);
        var client1 = container.GetBlobClient(documentAnalysisMessage.DocumentBlobName);        

        var command = CreateStartBronzeDocumentCommand(documentAnalysisMessage, client1.DownloadContent().Value.Content.ToStream());
        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            var bronzeStageQueueClient = queueClientService.GetQueueClient(storageOptionsService.Value.QueueNames[QueueStorageNames.BronzeStageQueue]);

            await bronzeStageQueueClient.SendMessageAsync(result.Value, CancellationToken.None);
            await documentAnalysisClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, CancellationToken.None);
        }

        Assert.NotNull(result);
    }

    private bool TryToDeserializeMessage(string message, out DocumentAnalysisMessage documentAnalysisMessage)
    {
        var success = false;
        documentAnalysisMessage = null!;

        try
        {
            documentAnalysisMessage = JsonSerializer.Deserialize<DocumentAnalysisMessage>(message)!;
            success = documentAnalysisMessage != null;
        }
        catch {}
        return success;
    }

    private DownloadBlobModel CreateDownloadBlobModel(DocumentAnalysisMessage documentAnalysisMessage, string blobContainerName)
    {
        return new DownloadBlobModel
        {
            BlobContainerName = blobContainerName,
            BlobName = documentAnalysisMessage.DocumentBlobName
        };
    }

    private StartBronzeDocumentCommand CreateStartBronzeDocumentCommand(DocumentAnalysisMessage documentAnalysisMessage, Stream blobContent)
    {        
        return new StartBronzeDocumentCommand
        {
            OperationId = new Guid(documentAnalysisMessage.OperationId),
            DocumentLocale = documentAnalysisMessage.Locale,
            DocumentPages = documentAnalysisMessage.Pages,
            DocumentContent = blobContent
        };
    }
}
