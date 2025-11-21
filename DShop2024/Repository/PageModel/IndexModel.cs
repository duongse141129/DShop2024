using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DShop2024.Repository
{
    public class IndexModel : PageModel
    {
        public PartialViewResult OnGetMyModalPartial()
        {
            return Partial("_MyModalPartial");
        }
    }
}
