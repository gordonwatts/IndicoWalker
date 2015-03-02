using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.Util
{
    /// <summary>
    /// Some stuff to help with common code between platforms
    /// </summary>
    public static class ExpirationOptions
    {
        /// <summary>
        /// Small item that will be displayed in the various combo-boxes.
        /// </summary>
        public class CacheTime
        {
            public string TimeString { get; set; }
            public TimeSpan Time { get; set; }

            public override string ToString()
            {
                return TimeString;
            }
        }

        /// <summary>
        /// Generate a list of items as options that can be used in a XAML UI list.
        /// </summary>
        /// <returns></returns>
        public static List<CacheTime> GetListExpirationOptions()
        {
            // Setup the Cache dropdown.
            var timeList = new List<CacheTime>()
            {
                new CacheTime() { Time = TimeSpan.FromDays(1), TimeString="One Day"},
                new CacheTime() { Time = TimeSpan.FromDays(7), TimeString="One Week"},
                new CacheTime() { Time = TimeSpan.FromDays(31), TimeString="One Month"},
                new CacheTime() {Time = TimeSpan.FromDays(31*2), TimeString="Two Months"},
                new CacheTime() { Time=TimeSpan.FromDays(31*3), TimeString="Three Months"},
                new CacheTime() { Time = TimeSpan.FromDays(31*6), TimeString="Six Months"},
                new CacheTime() { Time = TimeSpan.FromDays(365), TimeString="One Year"}
            };
            return timeList;
        }
    }
}
