using Newtonsoft.Json;

namespace Palaven.Model.Data.Documents.Metadata;

public class Instruction
{
    [JsonProperty("instruction_id")]
    public Guid InstructionId { get; set; }

    [JsonProperty("instruction")]
    public string InstructionText { get; set; } = default!;

    [JsonProperty("response")]
    public string Response { get; set; } = default!;

    [JsonProperty("type")]
    public string Type { get; set; } = default!;
}
