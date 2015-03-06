using IndicoInterface.NET;
using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// A list of meetings on indico (a category)
    /// </summary>
    class IndicoMeetingListRef : IMeetingListRef
    {
        private string addr;

        public IndicoMeetingListRef(string addr)
        {
            // TODO: Complete member initialization
            this.addr = addr;
        }

        /// <summary>
        /// Is this URL a valid meeting list reference (a pointer to a category)?
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static bool IsValid(string url)
        {
            return AgendaCategory.IsValid(url);
        }
    }
}
