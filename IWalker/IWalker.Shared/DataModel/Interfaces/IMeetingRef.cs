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
    }
}
