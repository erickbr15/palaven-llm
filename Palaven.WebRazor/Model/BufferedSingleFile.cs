using System.ComponentModel.DataAnnotations;

namespace Palaven.WebRazor2.Model;

public class BufferedSingleFile
{
    [Required]
    [Display(Name = "PDF Document")]
    public IFormFile FormFile { get; set; } = default!;

    [Display(Name = "User ID")]
    public string UserId { get; set; } = default!;

    [Display(Name = "Law name")]
    [StringLength(250, MinimumLength = 0)]
    public string Law { get; set; } = default!;

    [Display(Name = "Law acronym")]
    [StringLength(20, MinimumLength = 0)]
    public string Acronym { get; set; } = default!;

    [Display(Name = "Law year")]    
    public int Year { get; set; }
}
