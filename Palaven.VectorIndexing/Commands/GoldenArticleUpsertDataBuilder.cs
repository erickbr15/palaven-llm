using Liara.Clients.Pinecone.Model;
using Newtonsoft.Json.Linq;
using Palaven.Model.Ingest.Documents.Golden;

namespace Palaven.VectorIndexing.Commands;

public class GoldenArticleUpsertDataBuilder
{
    private UpsertDataModel? _target;

    public GoldenArticleUpsertDataBuilder NewWithNamespace(string dataNamespace)
    {
        _target = new UpsertDataModel
        {
            Namespace = dataNamespace,
            Vectors = new List<Vector>()
        };
        return this;
    }

    public GoldenArticleUpsertDataBuilder AddVector(JArray vector, Guid goldenArticleId, InstructionMetadata metadata)
    {
        var newVector = new Vector
        {
            Id = Guid.NewGuid().ToString(),
            Values = new List<double>(vector.Select(v=>(double)v).ToArray())
        };

        if (metadata != null)
        {
            newVector.Metadata.Add("golden_article_id", goldenArticleId);
            newVector.Metadata.Add("law_id", metadata.LawId);
            newVector.Metadata.Add("law_name", metadata.LawName);
            newVector.Metadata.Add("law_acronym", metadata.LawAcronym);
            newVector.Metadata.Add("law_year", metadata.LawYear);
            newVector.Metadata.Add("article_id", metadata.ArticleId);
            newVector.Metadata.Add("article", metadata.Article);
            newVector.Metadata.Add("llm_functions", string.Join("|", metadata.LlmFunctions));
        }

        _target!.Vectors.Add(newVector);

        return this;
    }

    public GoldenArticleUpsertDataBuilder AddVector(IList<double> vector, Guid goldenArticleId, InstructionMetadata metadata)
    {
        var newVector = new Vector
        {
            Id = Guid.NewGuid().ToString(),
            Values = vector
        };

        if (metadata != null)
        {
            newVector.Metadata.Add("golden_article_id", goldenArticleId);
            newVector.Metadata.Add("law_id", metadata.LawId);
            newVector.Metadata.Add("law_name", metadata.LawName);
            newVector.Metadata.Add("law_acronym", metadata.LawAcronym);
            newVector.Metadata.Add("law_year", metadata.LawYear);
            newVector.Metadata.Add("article_id", metadata.ArticleId);
            newVector.Metadata.Add("article", metadata.Article);
            newVector.Metadata.Add("llm_functions", string.Join("|", metadata.LlmFunctions));
        }

        _target!.Vectors.Add(newVector);

        return this;
    }

    public UpsertDataModel? Build()
    {
        return _target;
    }
}
