
using System;
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
        /// Get the list of sessions associated with this meeting or conference.
        /// </summary>
        ISession[] Sessions { get; }

        /// <summary>
        /// Get the start time of the meeting, including time zone information.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Turn the meeting into some sort of string that we can then use to re-constitute
        /// this IMeeting. This should be short - of order 60 characters (initial db size is 100).
        /// </summary>
        /// <returns></returns>
        string AsReferenceString();
    }
}
