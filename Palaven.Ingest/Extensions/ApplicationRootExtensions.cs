using Liara.Azure.AI;
using Microsoft.Extensions.DependencyInjection;
using Liara.Azure.BlobStorage;
using Palaven.Ingest.Commands;
using Palaven.Ingest.Services;
using Liara.Common.Http;
using Liara.Clients.OpenAI;
using Liara.Clients.Pinecone;
using Liara.Common;
using Palaven.Model.Ingest;

namespace Palaven.Ingest.Extensions;

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

    public static void AddIngestServices(this IServiceCollection services)
    {       
        services.AddSingleton<ICommandHandler<StartTaxLawIngestCommand, TaxLawDocumentIngestTask>, StartTaxLawIngestCommandHandler>();
        services.AddSingleton<ICommandHandler<CreateBronzeDocumentCommand, TaxLawDocumentIngestTask>, CreateBronzeDocumentCommandHandler>();
        services.AddSingleton<ICommandHandler<CreateSilverDocumentCommand, TaxLawDocumentIngestTask>, CreateSilverDocumentCommandHandler>();
        services.AddSingleton<ICommandHandler<CreateGoldenDocumentCommand, TaxLawDocumentIngestTask>, CreateGoldenDocumentCommandHandler>();        

        services.AddSingleton<IIngestTaxLawDocumentService, IngestTaxLawDocumentService>();
    }
}
