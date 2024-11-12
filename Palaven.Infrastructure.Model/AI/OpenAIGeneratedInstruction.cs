using Newtonsoft.Json;

namespace Palaven.Infrastructure.Model.AI;

public class OpenAIGeneratedInstruction
{
    [JsonProperty("instruction")]
    public string Instruction { get; set; } = default!;

    [JsonProperty("response")]
    public string Response { get; set; } = default!;

    [JsonProperty("legal_basis")]
    public string LegalBasis { get; set; } = default!;
}
