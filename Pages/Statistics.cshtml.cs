using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IRB.RevigoWeb.Pages
{
    public class StatisticsModel : PageModel
    {
		public string Key { get; set; }

		public void OnGet()
		{
			Key = WebUtilities.TypeConverter.ToString(Request.Query["key"]);

			if (string.IsNullOrEmpty(Key) || Key != Global.StatisticsKey)
			{
				Response.Redirect(Url.Content("~/"));
			}
		}
    }
}
