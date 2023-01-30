using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RevigoWeb
{
	public partial class PrivacyPolicy : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			RevigoMasterPage oMaster = (RevigoMasterPage)this.Master;
			oMaster.DisableGDPR();

			if (!this.IsPostBack)
			{
				if (oMaster.BasicCompliance)
				{
					this.chkTelemetry.Checked = false;
				}
				else
				{
					this.chkTelemetry.Checked = true;
				}
			}
		}

		protected void chkTelemetry_CheckedChanged(object sender, EventArgs e)
		{
			if (this.chkTelemetry.Checked)
			{
				((RevigoMasterPage)this.Master).AcceptAll();
			}
			else
			{
				((RevigoMasterPage)this.Master).AcceptBasic();
			}
			this.phFocus.Visible = true;
		}
	}
}