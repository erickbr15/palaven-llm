namespace Palaven.WebRazor.Model;

public class TaxLawFile
{
    public string UntrustedName { get; set; } = default!;
    public string Extension { get; set; } = default!;
    public byte[] Content { get; set; } = default!;
}
