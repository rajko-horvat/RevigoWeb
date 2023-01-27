using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RevigoWeb.Errors
{
	public partial class Error404 : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.TrySkipIisCustomErrors = true;
			Response.StatusCode = 404;
		}
	}
}