using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.Util
{
    static class StringHelpers
    {
        /// <summary>
        /// Take everything up to the first \r.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string TakeFirstLine(this string source)
        {
            var idx = source.IndexOf('\r');
            if (idx < 0)
                return source;

            return source.Substring(0, idx);
        }
    }
}
