using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase
{
    public class MeetingHelpers
    {

        /// <summary>
        /// Generate a meeting.
        /// </summary>
        /// <returns></returns>
        public static dummyMeetingRef CreateMeeting()
        {
            return new dummyMeetingRef();
        }
    }

    class dummyMeeting : IMeeting
    {
        public dummyMeeting()
        {
            Sessions = new ISession[] { new dummySession() };
        }
        public string Title
        {
            get { return "Meeting1"; }
        }

        public ISession[] Sessions { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string AsReferenceString()
        {
            return "meeting1";
        }
    }

    public class dummySession : ISession
    {
        public dummySession()
        {
            Talks = new ITalk[] { new dummyTalk() };
        }
        public ITalk[] Talks { get; set; }
    }

    class dummyTalk : ITalk
    {

        public string Title
        {
            get { return "talk 1"; }
        }

        public IFile TalkFile
        {
            get { return new dummyFile(); }
        }

        public bool Equals(ITalk other)
        {
            throw new NotImplementedException();
        }
    }

    class dummyFile : IFile
    {
        public bool IsValid
        {
            get { return true; }
        }

        public string FileType
        {
            get { return "pdf"; }
        }

        public string UniqueKey
        {
            get { return "talk1.pdf"; }
        }

        public Task<System.IO.StreamReader> GetFileStream()
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get { return "talk1.pdf"; }
        }
    }

    /// <summary>
    /// A pretty simple dummy meeting.
    /// </summary>
    public class dummyMeetingRef : IMeetingRef
    {
        public dummyMeetingRef()
        {
            NumberOfTimesFetched = 0;
        }
        public Task<IMeeting> GetMeeting()
        {
            NumberOfTimesFetched++;
            return Task.Factory.StartNew(() => new dummyMeeting() as IMeeting);
        }

        public string AsReferenceString()
        {
            return "meeting1";
        }

        public int NumberOfTimesFetched { get; set; }
    }
}
