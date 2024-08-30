namespace Palaven.Model.VectorIndexing.Commands;

public class UploadGoldenArticleToVectorIndexCommand
{
    public Guid TraceId { get; set; }
    public Guid GoldenArticleId { get; set; }
}
