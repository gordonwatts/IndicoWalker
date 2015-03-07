using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel.Interfaces
{
    /// <summary>
    /// An extended meeting reference. Includes some extra info like start time, end time, and title.
    /// This is often used as part of the calendar system.
    /// </summary>
    public interface IMeetingRefExtended
    {
        /// <summary>
        /// Get the title of this meeting
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Return the start time for this meeting
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Return the end time for this meeting
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// Get back the meeting we can use
        /// </summary>
        IMeetingRef Meeting { get; }
    }
}
