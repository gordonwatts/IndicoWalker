using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.Util
{
    static class DateUtils
    {
        /// <summary>
        /// Return true if the date is within the time span from the current time.
        /// </summary>
        /// <param name="delta">Time around which we return true. If dt - refTime is withing timespan (plus or minus).</param>
        /// <param name="dt">Date we are testing against</param>
        /// <param name="refTime">THe reference time</param>
        /// <returns></returns>
        public static bool Within(this DateTime dt, TimeSpan delta, DateTime? refTime = null)
        {
            DateTime marker = DateTime.Now;
            if (refTime.HasValue)
            {
                marker = refTime.Value;
            }
            var actualDelta = dt - marker;
            return (delta.TotalSeconds - Math.Abs(actualDelta.TotalSeconds)) > 0;
        }
    }
}
