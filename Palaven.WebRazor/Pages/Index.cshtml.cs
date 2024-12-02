using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Palaven.WebRazor2.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{        
    public IndexModel()
    {
    }                
}
