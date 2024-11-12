namespace Palaven.Application.Model.Ingest;

public class StartTaxLawIngestCommand
{
    public Guid UserId { get; set; }
    public string AcronymLaw { get; set; } = default!;
    public string NameLaw { get; set; } = default!;    
    public int YearLaw { get; set; }
    public string UntrustedFileName { get; set; } = default!;
    public string FileExtension { get; set; } = default!;
    public byte[] FileContent { get; set; } = default!;
}