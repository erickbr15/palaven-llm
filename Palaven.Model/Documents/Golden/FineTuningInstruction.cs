using Newtonsoft.Json;

namespace Palaven.Model.Documents.Golden;

public class FineTuningInstruction
{
    [JsonProperty("instruction_id")]
    public Guid InstructionId { get; set; }

    [JsonProperty("instruction")]
    public string Instruction { get; set; } = default!;

    [JsonProperty("response")]
    public string Response { get; set; } = default!;

    [JsonProperty("category")]
    public string Category { get; set; } = default!;

    [JsonProperty("metadata")]
    public InstructionMetadata Metadata { get; set; } = default!;
}
