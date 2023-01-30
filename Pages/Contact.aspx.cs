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
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Text;

namespace RevigoWeb
{
	public partial class Contact : System.Web.UI.Page
	{
		protected override void OnLoad(EventArgs e)
		{
			((RevigoMasterPage)this.Master).DisableGDPR();
			base.OnLoad(e);

			if (!IsCallback)
			{
				this.SetFocus(this.txtName);
			}
			else
			{

			}
		}

		protected void cmdSubmit_Click(object sender, EventArgs e)
		{
			CaptchaErrorEnum result = ValidateCaptcha(ConfigurationManager.AppSettings["RecaptchaPrivateKey"]);
			if (result != CaptchaErrorEnum.OK)
			{
				switch (result)
				{
					case CaptchaErrorEnum.Timeout:
						ShowMessage("reCaptcha Verification has failed. The timeout has occured");
						return;
					case CaptchaErrorEnum.VerificationFailed:
						ShowMessage("reCaptcha verification has failed. Please try again later.");
						return;
					case CaptchaErrorEnum.Unavailable:
						ShowMessage("Unable to verify reCaptcha. Please try again later.");
						return;
				}
				ShowMessage("Unknown reCaptcha verification error. Please try again later");
				return;
			}
			if (string.IsNullOrEmpty(this.txtName.Text))
			{
				ShowMessage("Please fill out your name");
				return;
			}
			if (string.IsNullOrEmpty(this.txtEmail.Text))
			{
				ShowMessage("Please fill out your e-mail address.");
				return;
			}
			if (string.IsNullOrEmpty(this.txtMessage.Text))
			{
				ShowMessage("Please fill out your message.");
				return;
			}
			if (!this.chkGDPR.Checked)
			{
				ShowMessage("Please agree with the GDPR consent.");
				return;
			}

			string sEmailServer = ConfigurationManager.AppSettings["EmailServer"];
			string sEmailTo = ConfigurationManager.AppSettings["DeveloperEmailTo"];
			string sEmailCc = ConfigurationManager.AppSettings["DeveloperEmailCC"];

			if (!string.IsNullOrEmpty(sEmailServer) && !string.IsNullOrEmpty(sEmailTo))
			{
				SmtpClient client = new SmtpClient(sEmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(this.txtEmail.Text, sEmailTo, "User message from Revigo",
					string.Format("Name: {0}\r\n\r\nMessage:\r\n{1}",
					this.txtName.Text, this.txtMessage.Text));
				if (!string.IsNullOrEmpty(sEmailCc))
					message.CC.Add(sEmailCc);

				try
				{
					client.Send(message);
				}
				catch (Exception ex)
				{
					Global.WriteToSystemLog(this.GetType().FullName, ex.Message);

					ShowMessage("The e-mail server has reported error, please check your e-mail address and try again.");
					return;
				}
			}


			this.pnlComment.Visible = false;
			ShowMessage("Your message has been sent successfully.");
		}

		private void ShowMessage(string message)
		{
			this.lblError.Text = string.Format("<p>{0}</p>", message);
			this.lblError.Visible = true;
		}

		private enum CaptchaErrorEnum
		{
			OK,
			Timeout,
			Unavailable,
			VerificationFailed
		}

		private CaptchaErrorEnum ValidateCaptcha(string privateKey)
		{
			try
			{
				HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify");
				//oRequest.UserAgent = "reCAPTCHA/ASP.NET";
				oRequest.Timeout = 30000; // 30 seconds
				oRequest.Method = "POST";
				oRequest.ContentType = "application/x-www-form-urlencoded";

				using (Stream requestStream = oRequest.GetRequestStream())
				{
					byte[] buffer = Encoding.ASCII.GetBytes(string.Format("secret={0}&response={1}&remoteip={2}",
					HttpUtility.HtmlEncode(privateKey),
					HttpUtility.HtmlEncode(Request.Params["g-recaptcha-response"].ToString()),
					HttpUtility.HtmlEncode(Request.UserHostAddress)));
					requestStream.Write(buffer, 0, buffer.Length);
				}
				// Get the response from the server
				HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
				if (oResponse.StatusCode == HttpStatusCode.OK)
				{
					StreamReader reader = new StreamReader(oResponse.GetResponseStream());
					// parse JSON response
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine().Replace("\"", "").Trim();
						if (line.StartsWith("success:", StringComparison.CurrentCultureIgnoreCase))
						{
							string value = line.Substring(8).Replace(",", "").Trim().ToLower();
							switch (value)
							{
								case "true":
									return CaptchaErrorEnum.OK;
								default:
									return CaptchaErrorEnum.VerificationFailed;
							}
						}
					}
				}
			}
			catch (WebException ex)
			{
				if (ex.Status == WebExceptionStatus.Timeout)
					return CaptchaErrorEnum.Timeout;
			}
			catch
			{ }

			return CaptchaErrorEnum.Unavailable;
		}
	}
}