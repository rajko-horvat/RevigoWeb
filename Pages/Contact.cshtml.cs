using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IRB.RevigoWeb.Pages
{
    public class ContactModel : PageModel
    {
		public string ErrorMessage = "";
		public bool SeccessfullySent = false;

		public void OnGet()
        {
        }
    }
}
