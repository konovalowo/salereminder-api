using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApi
{
    static class StringExtension
    {
        /// <summary>
        /// Removes extra whitespaces and \n from string and trims it.
        /// </summary>
        /// <param name="str">String</param>
        static public string RemoveExtraWS(this string str)
        {
            int wpcount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var ch in str)
            {
                if (ch == ' ' || ch == '\n')
                {
                    if (wpcount == 0)
                    {
                        sb.Append(' ');
                    }
                    wpcount++;
                }
                else
                {
                    wpcount = 0;
                    sb.Append(ch);
                }
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Removes query string from url.
        /// </summary>
        /// <param name="str">Url string</param>
        static public string RemoveQueryString(this string str)
        {
            int sgn = str.IndexOf('?');
            if (sgn != -1)
            {
                str = str.Substring(0, sgn);
            }
            return str;
        }

        static public string GetSecondDomain(this string url)
        {
            int doubleSlashIndex = url.IndexOf("//");
            if (doubleSlashIndex < 0) {
                return url;
            }
            int firstSlashIndex = url.IndexOf('/', doubleSlashIndex + 2);
            return url.Substring(0, firstSlashIndex);
        }
    }
}
