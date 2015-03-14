using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel.Categories
{
    /// <summary>
    /// The settings attached for an agenda category (basically an iCal feed of meeting agendas!).
    /// </summary>
    public class CategoryConfigInfo
    {
        /// <summary>
        /// Get/Set the meeting reference. This can't really be changed over time.
        /// </summary>
        public IMeetingListRef MeetingList { get; set; }

        /// <summary>
        /// Get/Set if this list of meetings should be shown on the home page
        /// </summary>
        public bool DisplayOnHomePage { get; set; }

        /// <summary>
        /// Get/Set the user title to be displayed for this meeting category list
        /// </summary>
        public string CategoryTitle { get; set; }
    }
}
