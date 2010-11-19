using System;
using System.Text;

namespace KeePass.IO.Utils
{
    /// <summary>
    /// Replace text with case insensitive support.
    /// Author: Michael Epner
    /// http://www.codeproject.com/KB/string/fastestcscaseinsstringrep.aspx?msg=1835929#xx1835929xx
    /// </summary>
    internal static class StringReplace
    {
        public static string Replace(string original,
            string pattern, string replacement,
            StringComparison comparisonType)
        {
            return Replace(original, pattern, replacement, comparisonType, -1);
        }

        public static string Replace(string original,
            string pattern, string replacement,
            StringComparison comparisonType,
            int stringBuilderInitialSize)
        {
            if (original == null)
                return null;

            if (String.IsNullOrEmpty(pattern))
                return original;


            var posCurrent = 0;
            var lenPattern = pattern.Length;
            var idxNext = original.IndexOf(pattern, comparisonType);
            var result = new StringBuilder(
                stringBuilderInitialSize < 0
                    ? Math.Min(4096, original.Length)
                    : stringBuilderInitialSize);

            while (idxNext >= 0)
            {
                result.Append(original,
                    posCurrent, idxNext - posCurrent);

                result.Append(replacement);

                posCurrent = idxNext + lenPattern;

                idxNext = original.IndexOf(pattern,
                    posCurrent, comparisonType);
            }

            result.Append(original, posCurrent,
                original.Length - posCurrent);

            return result.ToString();
        }
    }
}