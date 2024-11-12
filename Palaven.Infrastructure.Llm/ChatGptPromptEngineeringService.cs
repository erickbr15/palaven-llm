using Liara.Integrations.OpenAI.Chat;
using Palaven.Infrastructure.Abstractions.AI.Llm;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Infrastructure.Llm;

public class ChatGptPromptEngineeringService : IPromptEngineeringService<IEnumerable<Message>>
{
    private const string UserQueryMark = "{user_query}";
    private const string UserAdditionalInfoMark = "{additional_info}";

    public IEnumerable<Message> CreateAugmentedQueryPrompt(string instruction, IEnumerable<GoldenDocument> documents)
    {
        var relatedDocuments = documents != null && documents.Any() ?
            $"<additional_info>{string.Join("\n\n\n", documents.Select(r => r.ArticleContent))}</additional_info>" : 
            string.Empty;

        var userRolePrompt = Resources.ChatGptPromptTemplates.ChatGPTPromptUserRole
            .Replace(UserQueryMark, instruction)
            .Replace(UserAdditionalInfoMark, relatedDocuments);

        var messages = new List<Message>
        {
            new()
            {
                Role = "system",
                Content = Resources.ChatGptPromptTemplates.ChatGPTPromptSystemRole
            },
            new()
            {
                Role = "user",
                Content = userRolePrompt
            }
        };

        return messages;
    }

    public IEnumerable<Message> CreateFineTuningPrompt(string instruction, string response)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Message> CreateSimpleQueryPrompt(string instruction)
    {
        var messages = new List<Message>
        {
            new()
            {
                Role = "system",
                Content = Resources.ChatGptPromptTemplates.ChatGPTPromptSystemRole
            },
            new()
            {
                Role = "user",
                Content = Resources.ChatGptPromptTemplates.ChatGPTPromptUserRole
                    .Replace(UserQueryMark, instruction)
                    .Replace(UserAdditionalInfoMark, string.Empty)
            }
        };

        return messages;
    }
}
