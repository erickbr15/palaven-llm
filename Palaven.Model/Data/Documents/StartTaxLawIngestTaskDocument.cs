using Newtonsoft.Json;

namespace Palaven.Model.Data.Documents;

public class StartTaxLawIngestTaskDocument : EtlTaskDocument
{    
    [JsonProperty(PropertyName = "acronym_law")]
    public string AcronymLaw { get; set; } = default!;

    [JsonProperty(PropertyName = "name_law")]
    public string NameLaw { get; set; } = default!;

    [JsonProperty(PropertyName = "year_law")]
    public int YearLaw { get; set; }

    [JsonProperty(PropertyName = "untrusted_file_name")]
    public string UntrustedFileName { get; set; } = default!;

    [JsonProperty(PropertyName = "file_name")]
    public string FileName { get; set; } = default!;
}
