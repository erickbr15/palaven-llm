using Newtonsoft.Json;

namespace Palaven.Model.Ingest.Documents.Golden;

public class FineTuningInstruction
{
    [JsonProperty("instruction")]
    public string Instruction { get; set; } = default!;

    [JsonProperty("response")]
    public string Response { get; set; } = default!;

    [JsonProperty("category")]
    public string Category { get; set; } = default!;

    [JsonProperty("metadata")]
    public FineTuningInstructionMetadata Metadata { get; set; } = default!;
}
