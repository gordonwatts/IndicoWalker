using IWalker.DataModel.Interfaces;
using System;
using IWalker.DataModel.Inidco;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel
{
    /// <summary>
    /// Generate meeting reference lists from a url. For this, it is all indico all the time.
    /// </summary>
    class MeetingListGenerator : IMeetingListRefFactory
    {
        /// <summary>
        /// Given a url, assume it is an indico guy!
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IMeetingListRef GenerateMeetingListRef(string url)
        {
            return new IndicoMeetingListRef(url);
        }
    }
}
