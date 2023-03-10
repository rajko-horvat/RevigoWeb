using IRB.Revigo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace IRB.RevigoWeb.Pages
{
    public class ContactModel : PageModel
    {
		private static HttpClient oHttpClient = new HttpClient();
		public string? ErrorMessage = null;
		public bool SeccessfullySent = false;

		[BindProperty]
		public string? txtName { get; set; }

		[BindProperty]
		public string? txtEmail { get; set; }

		[BindProperty]
		public string? txtMessage { get; set; }

		[BindProperty]
		public string? chkGDPR { get; set; }

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

			if (!string.IsNullOrEmpty(Global.EmailServer) && !string.IsNullOrEmpty(Global.EmailTo))
			{
				SmtpClient client = new SmtpClient(Global.EmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(this.txtEmail, Global.EmailTo, "User message from Revigo",
					string.Format("Name: {0}\r\n\r\nMessage:\r\n{1}",
					this.txtName, this.txtMessage));
				if (!string.IsNullOrEmpty(Global.EmailCC))
					message.CC.Add(Global.EmailCC);

				try
				{
					client.Send(message);
				}
				catch (Exception ex)
				{
					Global.WriteToSystemLog($"{typeof(Global).GetType().Name}.OnPost", $"Message: '{ex.Message}', Stack trace: '{ex.StackTrace}'");

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

		private CaptchaErrorEnum ValidateCaptcha(string? privateKey)
		{
			if (string.IsNullOrEmpty(privateKey))
				return CaptchaErrorEnum.Unavailable;

			try
			{
				oHttpClient.Timeout = new TimeSpan(0, 0, 30);
				HttpRequestMessage oRequest = new HttpRequestMessage(HttpMethod.Post, "https://www.google.com/recaptcha/api/siteverify");
				oRequest.Method = HttpMethod.Post;

				IPAddress? address = Request.HttpContext.Connection.RemoteIpAddress;
				if (address == null)
				{
					return CaptchaErrorEnum.Unavailable;
				}
				oRequest.Content = new StringContent(string.Format("secret={0}&response={1}&remoteip={2}",
				HttpUtility.HtmlEncode(privateKey),
				HttpUtility.HtmlEncode(WebUtilities.TypeConverter.ToString(Request.Form["g-recaptcha-response"])),
				HttpUtility.HtmlEncode(address.ToString())), Encoding.UTF8, "application/x-www-form-urlencoded");

				// Get the response from the server
				HttpResponseMessage oResponse = oHttpClient.Send(oRequest);
				if (oResponse.StatusCode == HttpStatusCode.OK)
				{
					StreamReader reader = new StreamReader(oResponse.Content.ReadAsStream());
					// parse JSON response
					while (!reader.EndOfStream)
					{
						string? line = reader.ReadLine();
						if (!string.IsNullOrEmpty(line))
						{
							line = line.Replace("\"", "").Trim();
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
