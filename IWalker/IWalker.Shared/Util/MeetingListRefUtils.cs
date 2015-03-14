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
        /// <param name="meetings"></param>
        /// <returns></returns>
        public static IObservable<IMeetingRefExtended[]> FetchRecentMeetings (this IMeetingListRef meetings)
        {
            return Blobs.LocalStorage
                .GetAndFetchLatest(meetings.UniqueString, async () => (await meetings.GetMeetings(Settings.DaysBackToFetchMeetings)).ToArray(), null);
        }
    }
}
