using IndicoInterface.NET;
using IWalker.DataModel.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// A list of meetings on indico (a category)
    /// </summary>
    class IndicoMeetingListRef : IMeetingListRef
    {
        /// <summary>
        /// Get/Set the agenda. This is here so serialization works properly.
        /// </summary>
        public AgendaCategory aCategory { get; set; }

        /// <summary>
        /// Initialize with a particular URI
        /// </summary>
        /// <param name="uri"></param>
        public IndicoMeetingListRef(string uri)
        {
            aCategory = new AgendaCategory(uri);
        }

        /// <summary>
        /// Used for serialization
        /// </summary>
        public IndicoMeetingListRef()
        { }

        /// <summary>
        /// Is this URL a valid meeting list reference (a pointer to a category)?
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static bool IsValid(string url)
        {
            return AgendaCategory.IsValid(url);
        }

        /// <summary>
        /// Return a unique string which can be used as a cache.
        /// </summary>
        /// <remarks>We do it as a Uri, as that is unique at least on the internet...</remarks>
        [JsonIgnore]
        public string UniqueString
        {
            get { return aCategory.GetCagetoryUri(10).OriginalString; }
        }

        /// <summary>
        /// Return a list of meetings for this agenda.
        /// </summary>
        public async Task<IEnumerable<IMeetingRefExtended>> GetMeetings(int goingBackDays)
        {
            var al = new AgendaLoader(IndicoDataFetcher.Fetcher);
            var meetings = await al.GetCategory(aCategory, goingBackDays);

            return meetings.Select(m => new IndicoMeetingExtendedRef(m)).Cast<IMeetingRefExtended>().ToArray();
        }

        /// <summary>
        /// The meeting reference
        /// </summary>
        class IndicoMeetingExtendedRef : IMeetingRefExtended
        {
            /// <summary>
            /// Here for serialization. Get the underlying agenda which has the data.
            /// </summary>
            public AgendaInfoExtended aMeetingRef { get; set; }

            /// <summary>
            /// Init with a particular agenda.
            /// </summary>
            /// <param name="a"></param>
            public IndicoMeetingExtendedRef(AgendaInfoExtended a)
            {
                aMeetingRef = a;
            }

            /// <summary>
            /// For serialization initialization
            /// </summary>
            public IndicoMeetingExtendedRef()
            { }

            [JsonIgnore]
            public string Title { get { return aMeetingRef.Title; } }

            [JsonIgnore]
            public DateTime StartTime { get { return aMeetingRef.StartTime; } }

            [JsonIgnore]
            public DateTime EndTime { get { return aMeetingRef.EndTime; } }

            [JsonIgnore]
            public IMeetingRef Meeting
            {
                get { return new IndicoMeetingRef(aMeetingRef); }
            }
        }
    }
}
