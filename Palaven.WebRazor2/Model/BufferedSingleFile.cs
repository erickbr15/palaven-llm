using System.ComponentModel.DataAnnotations;

namespace Palaven.WebRazor2.Model;

public class BufferedSingleFile
{
    [Required]
    [Display(Name = "File")]
    public IFormFile FormFile { get; set; } = default!;

    [Display(Name = "Note")]
    [StringLength(50, MinimumLength = 0)]
    public string Note { get; set; } = default!;
}
