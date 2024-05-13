using Newtonsoft.Json;

namespace Palaven.Model.Ingest.Commands;

public class ChatCompletionInstructionsResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("instructions")]
    public IList<ChatCompletionInstruction> Instructions { get; set; } = new List<ChatCompletionInstruction>();
}
