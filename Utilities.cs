namespace IRB.RevigoWeb
{
    public static class WebUtilities
    {
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
        }
    }
}
