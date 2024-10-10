using Liara.Azure.AI;
using Liara.Clients.OpenAI;
using Liara.Clients.Pinecone;
using Liara.Common.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Palaven.Chat.Extensions;

public static class ApplicationRootExtensions
{
    public static void AddAIServices(this IServiceCollection services)
    {        
        services.AddOptions<OpenAiOptions>().BindConfiguration("OpenAi");
        services.AddOptions<PineconeOptions>().BindConfiguration("Pinecone");

        services.AddSingleton<IHttpProxy, HttpProxy>();
        services.AddSingleton<IOpenAiServiceClient, OpenAiServiceClient>();
        services.AddSingleton<IPineconeServiceClient, PineconeServiceClient>();
        services.AddSingleton<IDocumentLayoutAnalyzerService, DocumentLayoutAnalyzerService>();
    }

    public static void AddChatServices(this IServiceCollection services)
    {
        services.AddSingleton<IOpenAIChatService, OpenAIChatService>();
        services.AddSingleton<IGemmaChatService, GemmaChatService>();
    }
}
