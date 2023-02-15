using IRB.Revigo.Worker;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace IRB.RevigoWeb
{
    public static class WebUtilities
    {
		public static class TypeConverter
        {
            public static Boolean ToBoolean(object? value)
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

            public static uint ToUInt32(object? value)
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

            public static int ToInt32(object? value)
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

            public static long ToInt64(object? value)
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

            public static double ToDouble(object? value)
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

            public static string? ToString(object? value)
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

			public static string StringToJSON(string? text)
			{
				if (string.IsNullOrEmpty(text))
					return "null";

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
