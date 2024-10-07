using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Palaven.WebRazor2.Model;
using Palaven.WebRazor2.Utilities;

namespace Palaven.WebRazor2.Pages
{
    [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<IndexModel> _logger;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".pdf" };

        [BindProperty]
        public BufferedSingleFile FileUpload { get; set; }
        public string Result { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient)
        {
            _fileSizeLimit = 4000000;
            _logger = logger;
            _graphServiceClient = graphServiceClient;
        }


        public async Task OnGet()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();
            ViewData["GraphApiResult"] = user.DisplayName;
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            // Perform an initial check to catch FileUpload class
            // attribute violations.
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            var formFileContent =
                await FileHelpers.ProcessFormFile<BufferedSingleFile>(FileUpload.FormFile, ModelState, _permittedExtensions,_fileSizeLimit);

            // Perform a second check to catch ProcessFormFile method
            // violations. If any validation check fails, return to the
            // page.
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            // **WARNING!**
            // In the following example, the file is saved without
            // scanning the file's contents. In most production
            // scenarios, an anti-virus/anti-malware scanner API
            // is used on the file before making the file available
            // for download or for use by other systems. 
            // For more information, see the topic that accompanies 
            // this sample.

            var file = new AppFile
            {
                Content = formFileContent,
                UntrustedName = FileUpload.FormFile.FileName,
                Note = FileUpload.Note,
                Size = FileUpload.FormFile.Length,
                UploadDT = DateTime.UtcNow
            };            

            return RedirectToPage("./Index");
        }
    }
}
