using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RevigoWeb
{
	public partial class RevigoStats : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			((RevigoMasterPage)this.Master).DisableGDPR();
			((RevigoMasterPage)this.Master).DisableGoogle();
		}
	}
}