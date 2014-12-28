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
    }
}
