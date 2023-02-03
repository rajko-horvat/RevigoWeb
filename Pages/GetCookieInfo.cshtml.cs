using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace IRB.RevigoWeb.Pages
{
    public class GetCookieInfoModel : PageModel
    {
        public ContentResult OnGet()
        {
			// On a cross site request we can't get cookie info, but here we can :)
			GDPRTypeEnum eGDPRType = GDPR.GetGDPRState(HttpContext);

			return Content(eGDPRType.ToString(), "text/plain", Encoding.UTF8);
		}
    }
}
