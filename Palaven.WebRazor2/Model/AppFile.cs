using System.ComponentModel.DataAnnotations;

namespace Palaven.WebRazor2.Model;

public class AppFile
{
    public int Id { get; set; }

    public byte[] Content { get; set; } = default!;

    [Display(Name = "File Name")]
    public string UntrustedName { get; set; } = default!;

    [Display(Name = "Note")]
    public string Note { get; set; } = default!;

    [Display(Name = "Size (bytes)")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public long Size { get; set; }

    [Display(Name = "Uploaded (UTC)")]
    [DisplayFormat(DataFormatString = "{0:G}")]
    public DateTime UploadDT { get; set; }
}
