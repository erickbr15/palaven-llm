namespace Palaven.Infrastructure.Model.VectorIndexing;

public class VectorQuery
{
    public IEnumerable<string> Queries { get; set; } = default!;
    public string Namespace { get; set; } = default!;
    public int TopK { get; set; }
}
