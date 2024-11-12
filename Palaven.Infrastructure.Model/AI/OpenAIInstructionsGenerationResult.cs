using Newtonsoft.Json;

namespace Palaven.Infrastructure.Model.AI;

public class OpenAIInstructionsGenerationResult
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("instructions")]
    public IList<OpenAIGeneratedInstruction> Instructions { get; set; } = new List<OpenAIGeneratedInstruction>();
}
