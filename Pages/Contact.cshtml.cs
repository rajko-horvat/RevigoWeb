using IRB.Revigo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace IRB.RevigoWeb.Pages
{
    public class ContactModel : PageModel
    {
		public string ErrorMessage = "";
		public bool SeccessfullySent = false;

		[BindProperty]
		public string txtName { get; set; }

		[BindProperty]
		public string txtEmail { get; set; }

		[BindProperty]
		public string txtMessage { get; set; }

		[BindProperty]
		public string chkGDPR { get; set; }

		public void OnGet()
        {
        }

		public void OnPost() 
		{
			if (!string.IsNullOrEmpty(Global.RecaptchaPublicKey))
			{
				CaptchaErrorEnum result = ValidateCaptcha(Global.RecaptchaSecretKey);
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
			}
			if (string.IsNullOrEmpty(this.txtName))
			{
				ShowMessage("Please fill out your name");
				return;
			}
			if (string.IsNullOrEmpty(this.txtEmail))
			{
				ShowMessage("Please fill out your e-mail address.");
				return;
			}
			if (string.IsNullOrEmpty(this.txtMessage))
			{
				ShowMessage("Please fill out your message.");
				return;
			}
			if (string.IsNullOrEmpty(this.chkGDPR) || !this.chkGDPR.Equals("on", StringComparison.InvariantCultureIgnoreCase))
			{
				ShowMessage("Please agree with the GDPR consent.");
				return;
			}

			string sEmailServer = Global.EmailServer;
			string sEmailTo = Global.EmailTo;
			string sEmailCc = Global.EmailCC;

			if (!string.IsNullOrEmpty(sEmailServer) && !string.IsNullOrEmpty(sEmailTo))
			{
				SmtpClient client = new SmtpClient(sEmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(this.txtEmail, sEmailTo, "User message from Revigo",
					string.Format("Name: {0}\r\n\r\nMessage:\r\n{1}",
					this.txtName, this.txtMessage));
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

			this.SeccessfullySent = true;
			ShowMessage("Your message has been sent successfully.");
		}

		private void ShowMessage(string message)
		{
			this.ErrorMessage = message;
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
				oRequest.Timeout = 30000; // 30 seconds
				oRequest.Method = "POST";
				oRequest.ContentType = "application/x-www-form-urlencoded";

				using (Stream requestStream = oRequest.GetRequestStream())
				{
					IPAddress? address = Request.HttpContext.Connection.RemoteIpAddress;
					if (address == null)
					{
						return CaptchaErrorEnum.VerificationFailed;
					}
					byte[] buffer = Encoding.ASCII.GetBytes(string.Format("secret={0}&response={1}&remoteip={2}",
					HttpUtility.HtmlEncode(privateKey),
					HttpUtility.HtmlEncode(WebUtilities.TypeConverter.ToString(Request.Form["g-recaptcha-response"])),
					HttpUtility.HtmlEncode(address.ToString())));
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
