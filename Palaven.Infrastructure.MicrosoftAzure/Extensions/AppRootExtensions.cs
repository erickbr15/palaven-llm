using Azure;
using Liara.Integrations.Azure;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Infrastructure.Abstractions.AI;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Abstractions.Storage;
using Palaven.Infrastructure.MicrosoftAzure.AI;
using Palaven.Infrastructure.MicrosoftAzure.Messaging;
using Palaven.Infrastructure.MicrosoftAzure.Storage;

namespace Palaven.Infrastructure.MicrosoftAzure.Extensions;

public static class AppRootExtensions
{
    public static void AddAzureAIServices(this IServiceCollection services, IConfiguration configuration)
    {
        var aiServiceOptions = new AIServiceConnectionOptions();            
        configuration.Bind("AzureAI", aiServiceOptions);

        services.AddAzureClients(azureClientBuilder =>
        {
            azureClientBuilder.AddDocumentAnalysisClient(new Uri(aiServiceOptions.Endpoint), new AzureKeyCredential(aiServiceOptions.Key));
        });

        services.AddSingleton<IPdfDocumentAnalyzer, PdfDocumentAnalyzer>();
    }

    public static void AddAzureStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        const string STORAGE_CONNECTION_STRING_NAME = "AzureStorageLawDocs";

        var storageConnectionString = configuration.GetConnectionString(STORAGE_CONNECTION_STRING_NAME);
        services.AddAzureClients(azureClientBuilder =>
        {
            azureClientBuilder.AddBlobServiceClient(storageConnectionString).WithName(STORAGE_CONNECTION_STRING_NAME);
            azureClientBuilder.AddQueueServiceClient(storageConnectionString).WithName(STORAGE_CONNECTION_STRING_NAME);
        });
        
        services.AddOptions<StorageAccountOptions>().BindConfiguration("AzureStorage");
        services.AddSingleton<IQueueClientProvider, QueueClientProvider>();
        services.AddSingleton<IMessageQueueService, MessageQueueService>();
        services.AddSingleton<IEtlInboxStorage, EtlInboxStorage>();        
    }
}
