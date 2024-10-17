using Azure.Identity;
using Liara.Azure.Extensions;
using Liara.Azure.Storage;
using Liara.Clients.Extensions;
using Liara.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Data.Extensions;
using Palaven.Data.NoSql;
using Palaven.Ingest.Extensions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, configBuilder) =>
    {
        var appConfigurationEndpoint = Environment.GetEnvironmentVariable("AppConfigurationEndpoint");

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
        services.AddIngestCommands();
    });

        var host = hostBuilder.Build();


       


        //COMPLETE BRONZE DOCUMENT CREATION
        /*
        var storageOptions = host.Services.GetRequiredService<IOptions<AzureStorageOptions>>();
        var azureClientFactory = host.Services.GetRequiredService<IAzureClientFactory<QueueServiceClient>>();

        var options = storageOptions?.Value ?? throw new InvalidOperationException();
        var queueServiceClient = azureClientFactory.CreateClient("AzureStorageLawDocs") ?? throw new InvalidOperationException($"Unable to create the queue client for AzureStorageLawDocs");

        var documentAnalysisQueue = queueServiceClient.GetQueueClient(options.QueueNames[QueueStorageNames.DocumentAnalysisQueue]);

        var azureResponse = await documentAnalysisQueue.ReceiveMessageAsync(cancellationToken: default);
        var message = azureResponse.Value;

        if (!TryToDeserializeMessage(message.Body.ToString(), out var analysisMessage))
        {
            await documentAnalysisQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken: default);
            return;
        }

        var command = new CompleteBronzeDocumentCommand
        {
            OperationId = new Guid(analysisMessage.OperationId)
        };

        var commandHandler = host.Services.GetRequiredService<ICommandHandler<CompleteBronzeDocumentCommand, EtlTaskDocument>>();

        var result = await commandHandler.ExecuteAsync(command, cancellationToken: default);

        bool TryToDeserializeMessage(string message, out DocumentAnalysisMessage analysisMessage)
        {
            analysisMessage = null!;
            try
            {
                analysisMessage = JsonSerializer.Deserialize<DocumentAnalysisMessage>(message)!;
                if (analysisMessage == null)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /*
        var pageDocumentExtractor = host.Services.GetRequiredService<ITraceableCommand<ExtractLawDocumentPagesModel, IngestLawDocumentTaskInfo>>();
        var result = pageDocumentExtractor.ExecuteAsync(new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6"), 
            new ExtractLawDocumentPagesModel { OperationId = new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6") }, CancellationToken.None).GetAwaiter().GetResult();
        */


        // * CREATES THE SILVER DOCUMENT
        /*
        var extractCommand = host.Services.GetRequiredService<ITraceableCommand<ExtractLawDocumentArticlesModel, IngestLawDocumentTaskInfo>>();

        var traceId = new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6");
        var model = new ExtractLawDocumentArticlesModel { OperationId = traceId };
        var result = extractCommand.ExecuteAsync(traceId, model, CancellationToken.None).GetAwaiter().GetResult();
        */


        host.Run();
    }
}