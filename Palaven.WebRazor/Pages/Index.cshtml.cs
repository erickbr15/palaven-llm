using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Palaven.WebRazor2.Model;
using Palaven.WebRazor2.Utilities;
using Liara.Common;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;

namespace Palaven.WebRazor2.Pages
{
    [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".pdf" };
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<IndexModel> _logger;
        private readonly ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument> _startIngestCommandHandler;

        private Guid _userId;


        [BindProperty]
        public BufferedSingleFile InputModel { get; set; }
        public string Result { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient, ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument> startIngestCommandHandler)
        {
            _fileSizeLimit = 4000000;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            _startIngestCommandHandler = startIngestCommandHandler ?? throw new ArgumentNullException(nameof(startIngestCommandHandler));
        }

        public async Task OnGet()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();
            ViewData["GraphApiResult"] = $"{user.Id} - {user.DisplayName}";            
            _userId = new Guid(user.Id);            
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

            var formFile = await FileHelpers.ProcessFormFile<BufferedSingleFile>(InputModel.FormFile, ModelState, _permittedExtensions,_fileSizeLimit);

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
                UserId = _userId,
                UntrustedFileName = formFile.UntrustedName,
                FileExtension = formFile.Extension,
                FileContent = formFile.Content
            };

            var result = await _startIngestCommandHandler.ExecuteAsync(startIngestCommand, cancellationToken: CancellationToken.None);

            // **WARNING!**
            // In the following example, the file is saved without
            // scanning the file's contents. In most production
            // scenarios, an anti-virus/anti-malware scanner API
            // is used on the file before making the file available
            // for download or for use by other systems. 
            // For more information, see the topic that accompanies 
            // this sample.            

            return RedirectToPage("./Index");
        }
    }
}
