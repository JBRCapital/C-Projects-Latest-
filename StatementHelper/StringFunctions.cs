using System;

namespace AutoDocHelper
{
    public class StringFunctions
    {
        public static string JoinWith(string text1, object text2, string joiner)
        {

            if (text1 == null) { text1 = string.Empty; }
            if (text2 == null) { text2 = string.Empty; }

            if (text1.Length > 0 && text2.ToString().Length > 0)
            {
                return string.Concat(text1, joiner, text2);
            }

            return text1 + text2;
        }

        public static string getLeft(string value, int length)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value.Substring(0, Math.Min(length, value.Length));
        }
    }
}
