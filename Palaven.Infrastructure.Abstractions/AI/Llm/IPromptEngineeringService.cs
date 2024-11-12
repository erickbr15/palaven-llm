using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Infrastructure.Abstractions.AI.Llm;

public interface IPromptEngineeringService<TPrompt>
{
    TPrompt CreateSimpleQueryPrompt(string instruction);
    TPrompt CreateAugmentedQueryPrompt(string instruction, IEnumerable<GoldenDocument> documents);
    TPrompt CreateFineTuningPrompt(string instruction, string response);
}
