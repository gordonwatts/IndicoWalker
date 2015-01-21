using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel.Interfaces
{
    /// <summary>
    /// Meeting info
    /// </summary>
    public interface IMeeting
    {
        /// <summary>
        /// Get the title of the meeting
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Get the list of sessions assocated with this meeting or confernece.
        /// </summary>
        ISession[] Sessions { get; }
    }
}
