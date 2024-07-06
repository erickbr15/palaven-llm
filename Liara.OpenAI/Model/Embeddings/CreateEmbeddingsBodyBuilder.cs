namespace Liara.OpenAI.Model.Embeddings;

public class CreateEmbeddingsBodyBuilder
{
    private CreateEmbeddingsBody? _target;

    public CreateEmbeddingsBodyBuilder NewWithDefaults(string model, string user, IEnumerable<string> input)
    {
        _target = new CreateEmbeddingsBody
        {
            Input = input.ToArray(),
            Model = model,
            EncodingFormat = "float",
            User = user
        };
        return this;
    }

    public CreateEmbeddingsBody? Build()
    {
        return _target;
    }
}
