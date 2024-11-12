namespace Palaven.Infrastructure.Model.Persistence.Documents;

public static class EtlMetadataKeys
{
    public const string FileName = "FileName";
    public const string FileUntrustedName = "FileUntrustedName";
    public const string FileContentLocale = "FileContentLocale";
    public const string LawAcronym = "LawAcronym";
    public const string LawName = "LawName";
    public const string LawYear = "LawYear";
    public const string UserId = "UserId";
    public const string BronzeLayerCompleted = "BronzeLayerCompleted";
    public const string BronzeLayerProcessed = "BronzeLayerProcessed";
    public const string BronzeLayerDetectedPageCount = "BronzeLayerDetectedPageCount";
    public const string BronzeLayerIngestedPageCount = "BronzeLayerIngestedPageCount";   
    public const string SilverLayerExtractionCompleted = "SilverLayerExtractionCompleted";
    public const string SilverLayerExtractionProcessed = "SilverLayerExtractionProcessed";
    public const string SilverLayerExtractedArticleCount = "SilverLayerExtractedArticleCount ";
    public const string SilverLayerIngestedArticleCount = "SilverLayerIngestedArticleCount";
    public const string SilverLayerCurationCompleted = "SilverLayerCurationCompleted";
    public const string SilverLayerCurationProcessed = "SilverLayerCurationProcessed";

    public static readonly string[] DocumentMetadataKeys = new[]
    {
        FileName,
        FileUntrustedName,
        FileContentLocale,
        LawAcronym,
        LawName,
        LawYear
    };
}
