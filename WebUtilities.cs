using IRB.Revigo.Worker;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace IRB.RevigoWeb
{
    public static class WebUtilities
    {
		public static class Email
		{
			public static void SendEmailNotification(Exception ex)
			{
				if (!string.IsNullOrEmpty(Global.EmailServer) && !string.IsNullOrEmpty(Global.EmailFrom) && !string.IsNullOrEmpty(Global.EmailTo))
				{
					StringBuilder sMessage = new StringBuilder();
					sMessage.AppendFormat("Error occured in the Revigo application ({0}).", ex.GetType().FullName);
					sMessage.AppendLine();
					sMessage.AppendLine();
					sMessage.AppendLine();

					sMessage.AppendFormat("Error message: {0}", ex.Message);
					sMessage.AppendLine();
					sMessage.AppendLine();

					sMessage.AppendFormat("Stack trace: {0}", ex.StackTrace);
					sMessage.AppendLine();

					SmtpClient client = new SmtpClient(Global.EmailServer);
					client.EnableSsl = false;
					MailMessage message = new MailMessage(Global.EmailFrom, Global.EmailTo, "Notice from Revigo", sMessage.ToString());
					if (!string.IsNullOrEmpty(Global.EmailCC))
						message.CC.Add(Global.EmailCC);

					try
					{
						client.Send(message);
					}
					catch (Exception ex1)
					{
						Global.WriteToSystemLog(typeof(Global).FullName, ex1.Message);
					}
				}
				else
				{
					Global.WriteToSystemLog(typeof(Global).FullName, string.Format("Error occured in the Revigo application. Error message: '{0}'. Stack trace: '{1}'.",
						ex.Message, ex.StackTrace));
				}
			}

			public static void SendEmailNotification(RevigoWorker worker)
			{
				if (!string.IsNullOrEmpty(Global.EmailServer) && !string.IsNullOrEmpty(Global.EmailFrom) && !string.IsNullOrEmpty(Global.EmailTo))
				{
					StringBuilder sMessage = new StringBuilder();
					sMessage.AppendLine("Warning(s) and/or error(s) occured during processing of user data on http://revigo.irb.hr.");
					sMessage.AppendLine("The user data set has been attached.");
					sMessage.AppendLine();
					sMessage.AppendFormat("Parameters: CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}, RemoveObsolete = {4}",
						worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure, worker.RemoveObsolete);
					sMessage.AppendLine();
					if (worker.HasDeveloperWarnings)
					{
						sMessage.AppendLine();
						sMessage.AppendFormat("Warnings: {0}", TypeConverter.JoinStringArray(worker.DeveloperWarnings));
						sMessage.AppendLine();
					}
					if (worker.HasDeveloperErrors)
					{
						sMessage.AppendLine();
						sMessage.AppendFormat("Errors: {0}", TypeConverter.JoinStringArray(worker.DeveloperErrors));
						sMessage.AppendLine();
					}

					SmtpClient client = new SmtpClient(Global.EmailServer);
					client.EnableSsl = false;
					MailMessage message = new MailMessage(Global.EmailFrom, Global.EmailTo, "Notice from Revigo", sMessage.ToString());
					if (!string.IsNullOrEmpty(Global.EmailCC))
						message.CC.Add(Global.EmailCC);
					MemoryStream oStream = new MemoryStream(Encoding.UTF8.GetBytes(worker.Data));
					message.Attachments.Add(new Attachment(oStream, "dataset.txt", "text/plain;charset=UTF-8"));

					try
					{
						client.Send(message);
					}
					catch (Exception ex)
					{
						Global.WriteToSystemLog(typeof(Global).FullName, ex.Message);
					}

					oStream.Close();
				}
				else
				{
					Global.WriteToSystemLog(typeof(Global).FullName, string.Format("Warning(s) and/or error(s) occured during processing of user data on http://revigo.irb.hr; " +
						"CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}; RemoveObsolete = {4}; Warnings: {5}; Errors: {6}; Dataset: {7}",
						worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure, worker.RemoveObsolete,
						TypeConverter.JoinStringArray(worker.DeveloperWarnings), TypeConverter.JoinStringArray(worker.DeveloperErrors), worker.Data));
				}
			}

			public static void SendEmailNotification(RevigoWorker worker, Exception ex, string qType)
			{
				string sEmailServer = Global.EmailServer;
				string sEmailTo = Global.EmailTo;
				string sEmailCc = Global.EmailCC;

				if (!string.IsNullOrEmpty(sEmailServer) && !string.IsNullOrEmpty(Global.EmailFrom) && !string.IsNullOrEmpty(sEmailTo))
				{
					StringBuilder sMessage = new StringBuilder();
					sMessage.AppendLine("Error occured during exporting of the job results on http://revigo.irb.hr.");
					sMessage.AppendLine("The user data set has been attached.");
					sMessage.AppendLine();
					sMessage.AppendFormat("Query type: '{0}'", Convert.ToString(qType));
					sMessage.AppendLine();
					sMessage.AppendLine();
					sMessage.AppendFormat("Parameters: CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}, RemoveObsolete = {4}",
						worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure, worker.RemoveObsolete);
					sMessage.AppendLine();
					sMessage.AppendLine();

					sMessage.AppendFormat("Error message: {0}", ex.Message);
					sMessage.AppendLine();
					sMessage.AppendLine();

					sMessage.AppendFormat("Stack trace: {0}", ex.StackTrace);
					sMessage.AppendLine();

					SmtpClient client = new SmtpClient(sEmailServer);
					client.EnableSsl = false;
					MailMessage message = new MailMessage(sEmailTo, sEmailTo, "Notice from Revigo", sMessage.ToString());
					if (!string.IsNullOrEmpty(sEmailCc))
						message.CC.Add(sEmailCc);
					MemoryStream oStream = new MemoryStream(Encoding.UTF8.GetBytes(worker.Data));
					message.Attachments.Add(new Attachment(oStream, "dataset.txt", "text/plain;charset=UTF-8"));

					try
					{
						client.Send(message);
					}
					catch (Exception ex1)
					{
						Global.WriteToSystemLog(typeof(Email).FullName, ex1.Message);
					}

					oStream.Close();
				}
				else
				{
					Global.WriteToSystemLog(typeof(Email).FullName, string.Format("Error occured during exporting of the job results on http://revigo.irb.hr; " +
						"CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}; Warnings: {4}; Errors: {5}; Dataset: {6}",
						worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure,
						TypeConverter.JoinStringArray(worker.DeveloperWarnings), TypeConverter.JoinStringArray(worker.DeveloperErrors), worker.Data));
				}
			}
		}

		public static class TypeConverter
        {
            public static Boolean ToBoolean(object value)
            {
                bool bValue = false;

                if (value != null && value != DBNull.Value)
                {
                    try
                    {
                        bValue = Convert.ToBoolean(value);
                    }
                    catch
                    { }
                }

                return bValue;
            }

            public static uint ToUInt32(object value)
            {
                uint uiValue = 0;

                if (value != null && value != DBNull.Value)
                {
                    try
                    {
                        uiValue = Convert.ToUInt32(value);
                    }
                    catch
                    { }
                }

                return uiValue;
            }

            public static int ToInt32(object value)
            {
                int iValue = 0;

                if (value != null && value != DBNull.Value)
                {
                    try
                    {
                        iValue = Convert.ToInt32(value);
                    }
                    catch
                    { }
                }

                return iValue;
            }

            public static long ToInt64(object value)
            {
                long lValue = 0;

                if (value != null && value != DBNull.Value)
                {
                    try
                    {
                        lValue = Convert.ToInt64(value);
                    }
                    catch
                    { }
                }

                return lValue;
            }

            public static double ToDouble(object value)
            {
                double dValue = 0.0;

                if (value != null && value != DBNull.Value)
                {
                    try
                    {
                        dValue = Convert.ToDouble(value);
                    }
                    catch
                    { }
                }

                return dValue;
            }

            public static string? ToString(object value)
            {
                string? sValue = null;

                if (value != null && value != DBNull.Value)
                {
                    try
                    {
                        sValue = Convert.ToString(value);
                    }
                    catch
                    { }
                }

                return sValue;
            }

			public static string DoubleToJSON(double value)
			{
				if (double.IsNaN(value))
				{
					return "\"NaN\"";
				}

				return value.ToString(CultureInfo.InvariantCulture);
			}

			public static string StringToJSON(string text)
			{
				StringBuilder result = new StringBuilder();

				for (int i = 0; i < text.Length; i++)
				{
					char ch = text[i];
					if (ch < '\x20')
					{
						switch (ch)
						{
							case '\b':
								result.Append("\\b");
								break;
							case '\f':
								result.Append("\\f");
								break;
							case '\n':
								result.Append("\\n");
								break;
							case '\r':
								result.Append("\\r");
								break;
							case '\t':
								result.Append("\\t");
								break;
							default:
								result.AppendFormat("\\u{0:x4}", (int)ch);
								break;
						}
					}
					else if (ch > '\xff')
					{
						result.AppendFormat("\\u{0:x4}", (int)ch);
					}
					else
					{
						switch (ch)
						{
							case '\"':
								result.Append("\\\"");
								break;
							case '/':
								result.Append("\\/");
								break;
							case '\\':
								result.Append("\\\\");
								break;
							default:
								result.Append(ch);
								break;
						}
					}
				}

				return result.ToString();
			}

			public static string JoinStringArray(List<string> lines)
			{
				StringBuilder sbTemp = new StringBuilder();

				for (int i = 0; i < lines.Count; i++)
				{
					sbTemp.AppendLine(lines[i]);
				}

				return sbTemp.ToString();
			}

			public static string StringArrayToJSON(List<string> lines)
			{
				StringBuilder sbTemp = new StringBuilder();

				sbTemp.Append("[");
				for (int i = 0; i < lines.Count; i++)
				{
					if (i > 0)
						sbTemp.Append(",");
					sbTemp.AppendFormat("\"{0}\"", WebUtilities.TypeConverter.StringToJSON(lines[i]));
				}
				sbTemp.Append("]");

				return sbTemp.ToString();
			}

			public static string HtmlEncode(string text)
			{
				return WebUtility.HtmlEncode(text.Replace('\"', '\'').Replace("\n", "\\n").Replace("\r", "\\r"));
			}
		}
    }
}
