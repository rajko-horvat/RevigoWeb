using System;
using System.IO;
using System.Web.Hosting;

namespace RevigoWeb
{
	public partial class GetExample : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string sID = Convert.ToString(Request.QueryString["id"]);
			int iID = -1;

			if (!int.TryParse(sID, out iID))
			{
				ShowError("Parameters are missing or have wrong values.");
				return;
			}

			string sPath = HostingEnvironment.MapPath(string.Format("~/App_Data/Examples/example{0}.txt", iID));
			if (!File.Exists(sPath))
			{
				ShowError("Parameters are missing or have wrong values.");
				return;
			}

			Response.Clear();
			Response.ClearHeaders();
			Response.ContentType = "text/plain";
			StreamWriter writer = new StreamWriter(Response.OutputStream);
			StreamReader reader = new StreamReader(sPath);

			writer.Write(reader.ReadToEnd());

			reader.Close();
			writer.Flush();
			Response.End(); // response end must not be in try block
		}

		private void ShowError(string message)
		{
			Response.Clear();
			Response.ClearHeaders();
			Response.ContentType = "text/plain";
			StreamWriter writer = new StreamWriter(Response.OutputStream);
			writer.WriteLine(message);
			writer.Flush();
			Response.End(); // response end must not be in try block
		}
	}
}