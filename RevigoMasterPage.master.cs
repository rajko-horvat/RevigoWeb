using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace RevigoWeb
{
	public partial class RevigoMasterPage : System.Web.UI.MasterPage
	{
		bool bGDPRCompliance = false;
		bool bBasicCompliance = true;

		protected override void OnInit(EventArgs e)
		{
			HttpCookie cookie = Request.Cookies.Get("RevigoCookie");
			if (cookie != null)
			{
				string sGDPR = cookie.Value;
				if (!string.IsNullOrEmpty(sGDPR))
				{
					switch (sGDPR.ToUpper())
					{
						case "ALL":
							this.bGDPRCompliance = true;
							this.bBasicCompliance = false;
							break;
						case "BASIC":
							this.bGDPRCompliance = true;
							this.bBasicCompliance = true;
							break;
					}
					//cookie.Expires = DateTime.Now.AddDays(30d);
					//Response.Cookies.Add(cookie);
				}
			}

			if (!bGDPRCompliance)
			{
				// show GDPR message box
				this.pnlGDPR.Visible = true;
				this.phGDPR.Visible = true;
			}
			else
			{
				if (!this.bBasicCompliance)
				{
					this.phGoogle.Visible = true;
				}
			}

			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e)
		{
		}

		public bool GDPRCompliance
		{
			get
			{
				return this.bGDPRCompliance;
			}
		}

		public bool BasicCompliance
		{
			get
			{
				return this.bBasicCompliance;
			}
		}

		public void DisableGDPR()
		{
			// hide GDPR message box
			this.pnlGDPR.Visible = false;
			this.phGDPR.Visible = false;
		}

		public void DisableGoogle()
		{
			this.phGoogle.Visible = false;
		}

		public void AcceptAll()
		{
			HttpCookie cookie = new HttpCookie("RevigoCookie");
			cookie.Value = "ALL";
			cookie.Expires = DateTime.Now.AddDays(30d);
			Response.Cookies.Add(cookie);
			this.bBasicCompliance = false;
			this.phGoogle.Visible = true;
		}

		public void AcceptBasic()
		{
			HttpCookie cookie = new HttpCookie("RevigoCookie");
			cookie.Value = "BASIC";
			cookie.Expires = DateTime.Now.AddDays(30d);
			Response.Cookies.Add(cookie);
			this.bBasicCompliance = true;
			this.phGoogle.Visible = false;
		}

		protected void btnAcceptAll_Click(object sender, EventArgs e)
		{
			AcceptAll();
			DisableGDPR();
		}

		protected void btnAcceptBasic_Click(object sender, EventArgs e)
		{
			AcceptBasic();
			DisableGDPR();
		}
	}
}