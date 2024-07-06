namespace Liara.OpenAI;

public class OpenAiOptions
{
    public string ApiKey { get; set; } = default!;
    public string ChatEndpointUrl { get; set; } = default!;
    public string EmbeddingsEndpointUrl { get; set; } = default!;
    public string ChatGptModel { get; set; } = default!;
    public string EmbeddingsModel { get; set; } = default!;
}
