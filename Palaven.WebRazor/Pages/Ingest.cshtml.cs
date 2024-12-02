using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Model.Ingest;
using Palaven.WebRazor2.Model;
using Palaven.WebRazor2.Utilities;

namespace Palaven.WebRazor.Pages;

[AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
public class IngestModel : PageModel
{
    private readonly IStartIngestionChoreographyService _startIngestService;
    private readonly long _fileSizeLimit;
    private readonly string[] _permittedExtensions;

    public IngestModel(IStartIngestionChoreographyService startIngestService)
    {
        _fileSizeLimit = 4000000;
        _permittedExtensions = new string[] { ".pdf" };
        _startIngestService = startIngestService ?? throw new ArgumentNullException(nameof(startIngestService));
    }

    [BindProperty]
    public BufferedSingleFile InputModel { get; set; }
    public string Result { get; private set; }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        // Perform an initial check to catch FileUpload class
        // attribute violations.
        if (!ModelState.IsValid)
        {
            Result = "Please correct the form.";

            return Page();
        }

        var formFile = await FileHelpers.ProcessFormFile<BufferedSingleFile>(InputModel.FormFile, ModelState, _permittedExtensions, _fileSizeLimit);

        // Perform a second check to catch ProcessFormFile method
        // violations. If any validation check fails, return to the
        // page.
        if (!ModelState.IsValid)
        {
            Result = "Please correct the form.";
            return Page();
        }

        var startIngestCommand = new StartTaxLawIngestCommand
        {
            AcronymLaw = InputModel.Acronym,
            NameLaw = InputModel.Law,
            YearLaw = InputModel.Year,
            UserId = Guid.NewGuid(),
            UntrustedFileName = formFile.UntrustedName,
            FileExtension = formFile.Extension,
            FileContent = formFile.Content
        };

        var result = await _startIngestService.StartIngestionAsync(startIngestCommand, cancellationToken: CancellationToken.None);

        // **WARNING!**
        // In the following example, the file is saved without
        // scanning the file's contents. In most production
        // scenarios, an anti-virus/anti-malware scanner API
        // is used on the file before making the file available
        // for download or for use by other systems. 
        // For more information, see the topic that accompanies 
        // this sample.            

        return RedirectToPage("./Ingest");
    }
}
