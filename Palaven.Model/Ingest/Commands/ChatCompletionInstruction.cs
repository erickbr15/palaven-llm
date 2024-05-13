using Newtonsoft.Json;

namespace Palaven.Model.Ingest.Commands;

public class ChatCompletionInstruction
{
    [JsonProperty("instruction")]
    public string Instruction { get; set; } = default!;

    [JsonProperty("response")]
    public string Response { get; set; } = default!;

    [JsonProperty("legal_basis")]
    public string LegalBasis { get; set; } = default!;    
}
