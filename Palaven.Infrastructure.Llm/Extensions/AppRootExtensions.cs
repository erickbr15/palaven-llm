using Liara.Integrations.OpenAI.Chat;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Infrastructure.Abstractions.AI.Llm;
using Palaven.Infrastructure.Model.AI.Llm;

namespace Palaven.Infrastructure.Llm.Extensions;

public static class AppRootExtensions
{
    public static void AddLlmServices(this IServiceCollection services)
    {
        services.AddOptions<RetrievalOptions>().BindConfiguration("OpenAi:Retrieval");
        services.AddOptions<OpenAIChatOptions>().BindConfiguration("OpenAi:Chat");
        services.AddSingleton<IPromptEngineeringService<IEnumerable<Message>>, ChatGptPromptEngineeringService>();
        services.AddSingleton<IPromptEngineeringService<string>, GemmaPromptEngineeringService>();
    }
}
