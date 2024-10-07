using Liara.Azure.AI;
using Liara.Azure.BlobStorage;
using Liara.Clients.OpenAI;
using Liara.Clients.Pinecone;
using Liara.Common;
using Liara.Common.Http;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Model.VectorIndexing;
using Palaven.VectorIndexing.Commands;

namespace Palaven.VectorIndexing.Extensions;

public static class ApplicationRootExtensions
{
    public static void AddAIServices(this IServiceCollection services)
    {
        services.AddOptions<BlobStorageConnectionOptions>().BindConfiguration("BlobStorage");
        services.AddOptions<MultiServiceAiOptions>().BindConfiguration("AiServices");
        services.AddOptions<OpenAiOptions>().BindConfiguration("OpenAi");
        services.AddOptions<PineconeOptions>().BindConfiguration("Pinecone");

        services.AddSingleton<IHttpProxy, HttpProxy>();
        services.AddSingleton<IOpenAiServiceClient, OpenAiServiceClient>();
        services.AddSingleton<IPineconeServiceClient, PineconeServiceClient>();
        services.AddSingleton<IDocumentLayoutAnalyzerService, DocumentLayoutAnalyzerService>();
    }

    public static void AddVectorIndexingServices(this IServiceCollection services)
    {
        services.AddSingleton<ICommandHandler<UploadGoldenArticleToVectorIndexCommand, Guid>, UploadGoldenArticleToVectorIndexCommandHandler>();
        services.AddSingleton<IVectorIndexingService, VectorIndexingService>();
    }            
}
