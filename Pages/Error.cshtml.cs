using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IRB.RevigoWeb.Pages
{
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	[IgnoreAntiforgeryToken]
	public class ErrorModel : PageModel
    {
		public string ErrorMessage { get; set; }

        public void OnGet()
        {
			HandleError();
		}

		public void OnPost()
		{
			HandleError();
		}

		private void HandleError()
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
