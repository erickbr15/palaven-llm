using Newtonsoft.Json;

namespace Palaven.Model.Ingest.Documents.Golden;

public class InstructionMetadata
{
    [JsonProperty("law_id")]
    public Guid LawId { get; set; }

    [JsonProperty("law_name")]
    public string LawName { get; set; } = default!;

    [JsonProperty("law_acronym")]
    public string LawAcronym { get; set; } = default!;

    [JsonProperty("law_year")]
    public int LawYear { get; set; }

    [JsonProperty("article_id")]
    public Guid ArticleId { get; set; }

    [JsonProperty("article")]
    public string Article { get; set; } = default!;

    [JsonProperty("llm_functions")]
    public IList<string> LlmFunctions { get; set; } = new List<string>();
}
