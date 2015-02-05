using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.DataModel.Interfaces
{
    /// <summary>
    /// Access to the most recently viewed database for meetings.
    /// </summary>
    public interface IMRUDatabase
    {
        /// <summary>
        /// Given this meeting, mark it as having been recently visited in our database.
        /// </summary>
        /// <param name="meeting">The meeting itself</param>
        Task MarkVisitedNow(IMeeting meeting);
    }
}
