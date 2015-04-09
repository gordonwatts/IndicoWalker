using System.Threading.Tasks;

namespace IWalker.DataModel.Interfaces
{
    /// <summary>
    /// Reference to a meeting.
    /// </summary>
    public interface IMeetingRef
    {
        /// <summary>
        /// Return the meeting info for this meeting
        /// </summary>
        /// <returns></returns>
        Task<IMeeting> GetMeeting();

        /// <summary>
        /// Return a short string that represents this meeting.
        /// </summary>
        /// <returns></returns>
        string AsReferenceString();

        /// <summary>
        /// Get the URL for a meeting that can be used to view the meeting in a web browser.
        /// </summary>
        string WebURL { get; }
    }
}
