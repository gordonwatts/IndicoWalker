using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// Meeting reference for Indico
    /// </summary>
    class IndicoMeetingRef : IMeetingRef
    {
        /// <summary>
        /// The URL for this meeting.
        /// </summary>
        private string _url;

        /// <summary>
        /// Initialize with a URL
        /// </summary>
        /// <param name="url"></param>
        public IndicoMeetingRef(string url)
        {
            _url = url;
        }

        /// <summary>
        /// The meeting
        /// </summary>
        private class IndicoMeeting : IMeeting
        {
            public string Title
            {
                get { return "No way"; }
            }
        }


        /// <summary>
        /// Get the meeting info for this Indico agenda.
        /// </summary>
        /// <returns></returns>
        public async Task<IMeeting> GetMeeting()
        {
            return await Task<IMeeting>.Factory.StartNew(() => new IndicoMeeting());
        }
    }
}
