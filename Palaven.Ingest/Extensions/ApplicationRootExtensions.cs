using Liara.Azure.AI;
using Liara.CosmosDb;
using Microsoft.Extensions.DependencyInjection;
using Liara.Azure.BlobStorage;
using Palaven.Model.Ingest.Commands;
using Palaven.Ingest.Commands;
using Palaven.Ingest.Services;
using Liara.Common.Http;
using Liara.Clients.OpenAI;
using Liara.Clients.Pinecone;
using Liara.Common;

namespace Palaven.Ingest.Extensions;

public static class ApplicationRootExtensions
{
    public static void AddAIServices(this IServiceCollection services)
    {
        services.AddOptions<BlobStorageConnectionOptions>().BindConfiguration("BlobStorage");
        services.AddOptions<CosmosDbConnectionOptions>().BindConfiguration("CosmosDB");
        services.AddOptions<MultiServiceAiOptions>().BindConfiguration("AiServices");
        services.AddOptions<OpenAiOptions>().BindConfiguration("OpenAi");
        services.AddOptions<PineconeOptions>().BindConfiguration("Pinecone");

        services.AddSingleton<IHttpProxy, HttpProxy>();
        services.AddSingleton<IOpenAiServiceClient, OpenAiServiceClient>();
        services.AddSingleton<IPineconeServiceClient, PineconeServiceClient>();
        services.AddSingleton<IDocumentLayoutAnalyzerService, DocumentLayoutAnalyzerService>();
    }

    public static void AddIngestServices(this IServiceCollection services)
    {       
        services.AddSingleton<ICommandHandler<IngestLawDocumentModel, IngestLawDocumentTaskInfo>, StartIngestTaxLawDocument>();
        services.AddSingleton<ICommandHandler<ExtractLawDocumentPagesModel, IngestLawDocumentTaskInfo>, ExtractTaxLawDocumentPages>();
        services.AddSingleton<ICommandHandler<ExtractLawDocumentArticlesModel, IngestLawDocumentTaskInfo>, ExtractTaxLawDocumentArticles>();
        services.AddSingleton<ICommandHandler<CreateGoldenArticleDocumentModel, Guid>, CreateTaxLawGoldenArticleDocument>();        

        services.AddSingleton<IIngestTaxLawDocumentService, IngestTaxLawDocumentService>();
    }
}
