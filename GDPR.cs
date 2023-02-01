namespace IRB.RevigoWeb
{
    public enum GDPRTypeEnum
    {
        None,
        Basic,
        Full
    }

    public static class GDPR
    {
        public static GDPRTypeEnum GetGDPRState(HttpContext context)
        {
            GDPRTypeEnum eValue = GDPRTypeEnum.None;

            string? cookieValue = WebUtilities.TypeConverter.ToString(context.Request.Cookies["RevigoCookie"]);
            if (!string.IsNullOrEmpty(cookieValue))
            {
                switch (cookieValue.ToLower())
                {
                    case "full":
                        eValue = GDPRTypeEnum.Full;
                        cookieValue = "full";
                        break;
                    default:
                        eValue = GDPRTypeEnum.Basic;
                        cookieValue = "basic";
                        break;
                }
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(30);
                cookieOptions.SameSite = SameSiteMode.Strict;
                context.Response.Cookies.Append("RevigoCookie", cookieValue, cookieOptions);
            }

            return eValue;
        }
    }
}
