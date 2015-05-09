﻿using IWalker.DataModel.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
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
            Title = "session title";
        }
        public ITalk[] Talks { get; set; }


        public DateTime StartTime
        {
            get { return DateTime.Now; }
        }

        public string Title
        {
            get;
            set;
        }

        public string Id
        {
            get { return "1"; }
        }


        public bool IsPlaceHolderSession
        {
            get { return "<ad-hoc session>" == Title; }
        }
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

        [JsonIgnore]
        public DateTime StartTime
        {
            get { return DateTime.Now; }
        }

        [JsonIgnore]
        public DateTime EndTime
        {
            get { return DateTime.Now; }
        }


        public IFile[] AllTalkFiles
        {
            get { return new IFile[] { new dummyFile() }; }
        }
    }

    // A dummy file.
    class dummyFile : IFile
    {
        public int GetStreamCalled { get; private set; }
        private string _name;
        private string _url;
        public dummyFile(string url = "bogus", string name = "talk.pdf")
        {
            _name = name;
            _url = url;
            GetStreamCalled = 0;
            GetDateCalled = 0;
            DateToReturn = "this is the first";
            GetStream = () => Observable.FromAsync(() => Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(_url).AsTask())
                .SelectMany(f => f.OpenStreamForReadAsync())
                .Select(reader => new StreamReader(reader));

        }

        public bool IsValid { get { return true; } }

        public string FileType { get { return "pdf"; } }

        public string UniqueKey { get { return _name; } }

        public Func<IObservable<StreamReader>> GetStream { get; set; }

        public IObservable<StreamReader> GetFileStream()
        {
            GetStreamCalled++;
            if (GetStream == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                return GetStream();
            }
        }

        public string DisplayName { get { return _name; } }

        /// <summary>
        /// Date stamp to return.
        /// </summary>
        public string DateToReturn { get; set; }
        public IObservable<string> GetFileDate()
        {
            GetDateCalled++;
            return Observable.Return(DateToReturn);
        }

        public int GetDateCalled { get; private set; }
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

        [JsonIgnore]
        public int NumberOfTimesFetched { get; set; }


        [JsonIgnore]
        public string WebURL
        {
            get { throw new NotImplementedException(); }
        }
    }


    class myMeetingListRef : IMeetingListRef
    {
        public int Counter { get; private set; }

        public myMeetingListRef()
        {
            Counter = 0;
        }

        /// <summary>
        /// Return a dummy set of meetings.
        /// </summary>
        /// <param name="goingBackDays"></param>
        /// <returns></returns>
        public Task<IEnumerable<IMeetingRefExtended>> GetMeetings(int goingBackDays)
        {
            Counter++;
            return Task.Factory.StartNew(() =>
            {
                return new IMeetingRefExtended[] { new anExtendedMeetingRef("meeting1"), new anExtendedMeetingRef("meeting2") } as IEnumerable<IMeetingRefExtended>;
            });
        }


        public string UniqueString
        {
            get { return "111222"; }
        }
    }

    /// <summary>
    /// Dummy extended meeting for testing.
    /// </summary>
    /// <remarks>This will get serialized, so we have to make sure it is "readY".</remarks>
    class anExtendedMeetingRef : IMeetingRefExtended
    {
        public anExtendedMeetingRef(string mname)
        {
            Title = mname;
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            Meeting = null;
        }

        public string Title { get; set; }

        public System.DateTime StartTime { get; set; }

        public System.DateTime EndTime { get; set; }

        public IMeetingRef Meeting { get; set; }
    }
}
