using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RevigoWeb
{
	public partial class TermsOfUse : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			((RevigoMasterPage)this.Page.Master).DisableGDPR();
		}
	}
}