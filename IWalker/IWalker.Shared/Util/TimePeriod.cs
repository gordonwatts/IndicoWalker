using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.Util
{
    /// <summary>
    /// Very simple time period class.
    /// </summary>
    public class TimePeriod
    {
        public TimePeriod(DateTime start, DateTime end)
        {
            StartTime = start;
            EndTime = end;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="talkTime"></param>
        public TimePeriod(TimePeriod talkTime)
        {
            StartTime = talkTime.StartTime;
            EndTime = talkTime.EndTime;
        }

        /// <summary>
        /// Returns true if it contains the time.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Contains (DateTime t)
        {
            return StartTime < t
                && EndTime > t;
        }

        /// <summary>
        /// The end time.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// The start time.
        /// </summary>
        public DateTime StartTime { get; set; }
    }
}
