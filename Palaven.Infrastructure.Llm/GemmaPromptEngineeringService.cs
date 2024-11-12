using Palaven.Infrastructure.Abstractions.AI.Llm;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Infrastructure.Llm;

public class GemmaPromptEngineeringService : IPromptEngineeringService<string>
{            
    public string CreateAugmentedQueryPrompt(string instruction, IEnumerable<GoldenDocument> documents)
    {
        if (documents == null || !documents.Any())
        {
            var prompt = this.CreateSimpleQueryPrompt(instruction);
            return prompt;
        }

        var augmentedQuery = Resources.GemmaPromptTemplates.GemmaAugmentedQuery
            .Replace("{articles}", string.Join("", documents.Select(a => $"<article>{a.ArticleContent}</article>")))
            .Replace("{instruction}", instruction);

        return augmentedQuery;
    }

    public string CreateFineTuningPrompt(string instruction, string response)
    {
        throw new NotImplementedException();
    }

    public string CreateSimpleQueryPrompt(string instruction)
    {
        if(string.IsNullOrWhiteSpace(instruction))
        {
            throw new ArgumentNullException(nameof(instruction));
        }

        var prompt = Resources.GemmaPromptTemplates.GemmaSimpleQuery.Replace("{instruction}", instruction);

        return prompt;
    }
}
