using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IRB.RevigoWeb.Pages
{
	[IgnoreAntiforgeryToken]
	public class ErrorModel : PageModel
    {
		public string ErrorMessage { get; set; }
        public void OnGet()
        {
			var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			this.ErrorMessage = "Oops, something went wrong.";

			if (exceptionHandler != null)
			{
				Exception error = exceptionHandler.Error;

				if (!error.Message.Contains("Request timed out") &&
					!error.Message.Contains("invalid webresource request") &&
					!error.Message.Contains("potentially dangerous"))
				{
					WebUtilities.Email.SendEmailNotification(error);
				}
			}
		}
    }
}
