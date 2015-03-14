using Akavache;
using IWalker.DataModel.Interfaces;
using System;
using System.Linq;

namespace IWalker.Util
{
    /// <summary>
    /// Centralize and helpers for dealing with IMeetingListRef objects.
    /// </summary>
    static class MeetingListRefUtils
    {
        /// <summary>
        /// Get the most recent meetings, and re-fetch as well if they are in the cache.
        /// </summary>
        /// <param name="meetings">Meetings reference we should go after</param>
        /// <param name="updateAlways">If true, we will always update the meeting list. Otherwise we won't do it if we've recently done it</param>
        /// <returns></returns>
        public static IObservable<IMeetingRefExtended[]> FetchAndUpdateRecentMeetings (this IMeetingListRef meetings, bool updateAlways = true)
        {
            Func<DateTimeOffset, bool> refetchFunc = null;

            if (!updateAlways)
            {
                refetchFunc = lasttime => (DateTime.Now - lasttime).TotalHours > Settings.MeetingCategoryStaleHours;
            }

            return Blobs.LocalStorage
                .GetAndFetchLatest(meetings.UniqueString, async () => (await meetings.GetMeetings(Settings.DaysBackToFetchMeetings)).ToArray(), refetchFunc);
        }
    }
}
