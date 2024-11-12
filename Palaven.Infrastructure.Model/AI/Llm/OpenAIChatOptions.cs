namespace Palaven.Infrastructure.Model.AI.Llm;

public class OpenAIChatOptions
{
    public string Model { get; set; } = default!;
    public decimal? Temperature { get; set; }
}
