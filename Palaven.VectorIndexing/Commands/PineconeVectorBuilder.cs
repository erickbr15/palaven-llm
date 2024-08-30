using Liara.Clients.Pinecone.Model;
using Newtonsoft.Json.Linq;
using Palaven.Model.Documents.Golden;

namespace Palaven.VectorIndexing.Commands;

public class PineconeVectorBuilder
{
    private Vector? _target;

    public PineconeVectorBuilder NewWith(JArray vector, string goldenArticleId, InstructionMetadata metadata)
    {
        _target = new Vector
        {
            Id = Guid.NewGuid().ToString(),
            Values = new List<double>(vector.Select(v => (double)v).ToArray())
        };

        if (metadata != null)
        {
            _target.Metadata.Add("golden_article_id", new Guid(goldenArticleId));
            _target.Metadata.Add("law_id", metadata.LawId);
            _target.Metadata.Add("law_name", metadata.LawName);
            _target.Metadata.Add("law_acronym", metadata.LawAcronym);
            _target.Metadata.Add("law_year", metadata.LawYear);
            _target.Metadata.Add("article_id", metadata.ArticleId);
            _target.Metadata.Add("article", metadata.Article);
            _target.Metadata.Add("llm_functions", string.Join("|", metadata.LlmFunctions));
        }
        return this;
    }

    public Vector Build()
    {
        return _target ?? throw new InvalidOperationException("Vector not initialized");
    }
}
