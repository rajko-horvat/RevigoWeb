using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IRB.RevigoWeb.Pages
{
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public class ErrorCodeModel : PageModel
    {
		public int OriginalStatusCode { get; set; }
		public string? OriginalPathAndQuery { get; set; }

		public void OnGet(int statusCode)
        {
			OriginalStatusCode = statusCode;

			var statusCodeReExecuteFeature =
				HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

			OriginalPathAndQuery = null;

			if (statusCodeReExecuteFeature is not null)
			{
				OriginalPathAndQuery = string.Join(
					statusCodeReExecuteFeature.OriginalPathBase,
					statusCodeReExecuteFeature.OriginalPath,
					statusCodeReExecuteFeature.OriginalQueryString);
			}
		}
    }
}
