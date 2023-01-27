using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RevigoWeb
{
	public partial class FAQ : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			((RevigoMasterPage)this.Master).DisableGDPR();

			string sPath = HostingEnvironment.MapPath("~/App_Data/Examples/example1.txt");
		}
	}
}