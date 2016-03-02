using IndicoInterface.NET;
using IndicoInterface.NET.SimpleAgendaDataModel;
using IWalker.DataModel.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Web.Http.Headers;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// Meeting reference for Indico
    /// </summary>
    public class IndicoMeetingRef : IMeetingRef
    {
        /// <summary>
        /// The URL for this meeting.
        /// </summary>
        private AgendaInfo _info;

        /// <summary>
        /// Initialize with a URL
        /// </summary>
        /// <param name="url"></param>
        public IndicoMeetingRef(string url)
        {
            _info = new AgendaInfo(url);
        }

        public IndicoMeetingRef(AgendaInfo ag)
        {
            _info = ag;
        }

        /// <summary>
        /// The constructor to be used with serialization
        /// </summary>
        public IndicoMeetingRef()
        { }

        /// <summary>
        /// Implement the interface to a session.
        /// </summary>
        private class IndicoSesson : ISession
        {
            /// <summary>
            /// Get/Set the session. Don't touch - taken care of internally (and by the serializer).
            /// </summary>
            public Session aSession { get; set; }

            /// <summary>
            /// Cache the session for later use.
            /// </summary>
            /// <param name="s"></param>
            public IndicoSesson(Session s, string meetingID)
            {
                this.aSession = s;
                _key = meetingID;
            }

            private ITalk[] _talks = null;

            /// <summary>
            /// Get the talks for this meeting.
            /// </summary>
            [JsonIgnore]
            public ITalk[] Talks
            {
                get
                {
                    if (_talks == null)
                    {
                        _talks = aSession.Talks.Select(t => new IndicoTalk(t, _key)).ToArray();
                    }
                    return _talks;
                }
            }

            /// <summary>
            /// Track the key for the meeting.
            /// </summary>
            public string _key { get; set; }

            [JsonIgnore]
            public DateTime StartTime
            {
                get { return aSession.StartDate; }
            }

            [JsonIgnore]
            public string Title
            {
                get { return aSession.Title; }
            }

            [JsonIgnore]
            public string Id
            {
                get { return aSession.ID; }
            }

            /// <summary>
            /// If this session is one that was made up during the parsing just to make things fit in a uniform model,
            /// then we should "ignore" it. The code is done just by the title.
            /// </summary>
            [JsonIgnore]
            public bool IsPlaceHolderSession
            {
                get { return aSession.Title == "<ad-hoc session>"; }
            }
        }

        /// <summary>
        /// Implement the interface to a talk
        /// </summary>
        public class IndicoTalk : ITalk
        {
            /// <summary>
            /// Get/Set the indico talk that this is associated with. Don't touch - for the serializer.
            /// </summary>
            public Talk aTalk { get; set; }

            /// <summary>
            /// Track the unique talk ID.
            /// </summary>
            /// <remarks>Public so that it can be written out.</remarks>
            public string Key { get; set; }

            /// <summary>
            /// Init the talk with a particular meeting ID and file (s).
            /// </summary>
            /// <param name="t"></param>
            /// <param name="meetingUniqueID"></param>
            public IndicoTalk(Talk t, string meetingUniqueID)
            {
                this.aTalk = t;
                MeetingUniqueID = meetingUniqueID;
                Key = GenerateTalkFileKey(t.SlideURL);
            }

            /// <summary>
            /// Generate a talk file key
            /// </summary>
            /// <param name="t"></param>
            private string GenerateTalkFileKey(string url)
            {
                return string.Format("{0}/{1}/{2}", MeetingUniqueID, aTalk.ID, url);
            }

            /// <summary>
            /// Get the talk title.
            /// </summary>
            [JsonIgnore]
            public string Title
            {
                get { return aTalk.Title; }
            }

            /// <summary>
            /// Track the indico file.
            /// </summary>
            private IndicoFile _file;

            /// <summary>
            /// Return the default file associated with this talk.
            /// </summary>
            [JsonIgnore]
            public IFile TalkFile
            {
                get
                {
                    if (_file == null)
                    {
                        // We have to be a little fast/loose with the default file here.
                        if (aTalk.SlideURL != null)
                        {
                            var defaultFile = new TalkMaterial()
                            {
                                URL = aTalk.SlideURL,
                                DisplayFilename = aTalk.DisplayFilename,
                                FilenameExtension = aTalk.FilenameExtension,
                                MaterialType = "Slides"
                            };
                            _file = new IndicoFile(defaultFile, Key);
                        }
                        else
                        {
                            _file = new IndicoFile(null, Key);
                        }
                    }
                    return _file;
                }
            }

            /// <summary>
            /// Is this talk the same as that other talk?
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(ITalk other)
            {
                var italk = other as IndicoTalk;
                if (italk == null)
                    return false;

                if (italk.aTalk.ID != aTalk.ID)
                    return false;

                if (italk.aTalk.Title != aTalk.Title)
                    return false;

                if (italk.aTalk.StartDate != aTalk.StartDate)
                    return false;

                if (italk.aTalk.EndDate != aTalk.EndDate)
                    return false;

                if (italk.aTalk.SlideURL != aTalk.SlideURL)
                    return false;

                // Next, there are other minor things that might get updated that will require us to re-do this talk.

                return true;
            }

            /// <summary>
            /// Start time of the talk
            /// </summary>
            [JsonIgnore]
            public DateTime StartTime { get { return aTalk.StartDate; } }

            /// <summary>
            /// End time of the talk
            /// </summary>
            [JsonIgnore]
            public DateTime EndTime { get { return aTalk.EndDate; } }

            /// <summary>
            /// Returns a list of all talk files
            /// </summary>
            [JsonIgnore]
            public IFile[] AllTalkFiles
            {
                get {
                    if (_allTalkFiles == null)
                    {
                        _allTalkFiles = aTalk.AllMaterial.Select(t => new IndicoFile(t, GenerateTalkFileKey(t.URL))).ToArray();
                    }
                    return _allTalkFiles;
                }
            }
            private IFile[] _allTalkFiles = null;

            /// <summary>
            /// A unique meeting ID that is written out and serialized
            /// </summary>
            public string MeetingUniqueID { get; set; }

            /// <summary>
            /// Return all talk files that are attached as sub-talks.
            /// </summary>
            [JsonIgnore]
            public ITalk[] SubTalks
            {
                get
                {
                    if (_subTalks == null)
                    {
                        _subTalks = aTalk.SubTalks == null ?
                            new ITalk[0]
                            : aTalk.SubTalks.Select(st => new IndicoTalk(st, "neverUsedIHope")).ToArray();
                    }
                    return _subTalks;
                }
            }
            private ITalk[] _subTalks = null;

            /// <summary>
            /// Get a list of the authors for this talk.
            /// </summary>
            [JsonIgnore]
            public string[] Speakers
            {
                get { return aTalk.Speakers; }
            }

        }

        /// <summary>
        /// Represents a file on the indico server, a component of a talk in an agenda somewhere.
        /// </summary>
        public class IndicoFile : IFile
        {
            /// <summary>
            /// Get/Set the URL for the file.
            /// </summary>
            public TalkMaterial _aFile { get; set; }

            /// <summary>
            /// Initialize with the URL for this talk
            /// </summary>
            /// <param name="fileUri"></param>
            public IndicoFile(TalkMaterial talkInfo, string uniqueKey)
            {
                _aFile = talkInfo;
                UniqueKey = uniqueKey;
            }

            /// <summary>
            /// Does this object have any hope of fetching a file?
            /// </summary>
            [JsonIgnore]
            public bool IsValid
            {
                get { return _aFile != null && !string.IsNullOrWhiteSpace(_aFile.FilenameExtension); }
            }

            /// <summary>
            /// Return a stream that can be used to read over the net along with the date from the
            /// server. The GetFileDate method should return the same date.
            /// </summary>
            /// <returns></returns>
            public IObservable<Tuple<string, StreamReader>> GetFileStream()
            {
                return Observable.FromAsync(() => IndicoDataFetcher.Fetcher.GetDataAndHeadersFromURL(new Uri(_aFile.URL)))
                    .Select(info => Tuple.Create(ExtractLastModifiedHeader(info.Item1), info.Item2));
            }

            /// <summary>
            /// Given headers, extract the date we will use to mark when this file was last modified on the server.
            /// </summary>
            /// <param name="dt"></param>
            /// <returns></returns>
            private string ExtractLastModifiedHeader(HttpContentHeaderCollection dt)
            {
                return dt.LastModified.HasValue ? dt.LastModified.Value.ToString() : "";
            }

            /// <summary>
            /// Given the URL, get the header info.
            /// </summary>
            /// <returns></returns>
            public IObservable<string> GetFileDate()
            {
                return Observable.FromAsync(() => IndicoDataFetcher.Fetcher.GetContentHeadersFromUrl(new Uri(_aFile.URL)))
                    .Select(ExtractLastModifiedHeader);
            }

            /// <summary>
            /// Return the file type based on the URL.
            /// </summary>
            [JsonIgnore]
            public string FileType
            {
                get {
                    if (!IsValid)
                        return "";
                    return _aFile.FilenameExtension.Substring(1);
                }
            }

            /// <summary>
            /// Unique key that we can use to find this file.
            /// </summary>
            public string UniqueKey { get; set; }

            /// <summary>
            /// The display name we can use
            /// </summary>
            [JsonIgnore]
            public string DisplayName
            {
                get { return _aFile.DisplayFilename; }
            }
        }

        /// <summary>
        /// The meeting
        /// </summary>
        public class IndicoMeeting : IMeeting
        {
            /// <summary>
            /// Hold onto a complete agenda internally.
            /// </summary>
            /// <remarks>Public so the serializer works properly.</remarks>
            public Meeting aAgenda { get; set; }

            /// <summary>
            /// Internal cache of the sessions for this object.
            /// </summary>
            private IndicoSesson[] _sessons = null;

            /// <summary>
            /// Memorize the short string for this item
            /// </summary>
            public string aShortString { get; set; }

            /// <summary>
            /// Start up and cache the meeting agenda.
            /// </summary>
            /// <param name="agenda"></param>
            public IndicoMeeting(Meeting agenda, string shortString)
            {
                this.aAgenda = agenda;
                aShortString = shortString;
            }

            /// <summary>
            /// Get the title for this meeting.
            /// </summary>
            [JsonIgnore]
            public string Title
            {
                get { return aAgenda.Title; }
            }

            /// <summary>
            /// Return the date of the meeting
            /// </summary>
            [JsonIgnore]
            public DateTime StartTime
            {
                get { return aAgenda.StartDate; }
            }

            /// <summary>
            /// Return the date when the meeting ends.
            /// </summary>
            [JsonIgnore]
            public DateTime EndTime
            {
                get { return aAgenda.EndDate; }
            }

            /// <summary>
            /// Get the sessions. Populate them if need be!
            /// </summary>
            [JsonIgnore]
            public ISession[] Sessions
            {
                get
                {
                    if (_sessons == null)
                    {
                        _sessons = aAgenda.Sessions.Select(s => new IndicoSesson(s, aShortString)).ToArray();
                    }
                    return _sessons;
                }
            }

            /// <summary>
            /// Return the short flufable string.
            /// </summary>
            /// <returns></returns>
            public string AsReferenceString()
            {
                return aShortString;
            }
        }

        /// <summary>
        /// Get the meeting info for this Indico agenda.
        /// </summary>
        /// <returns></returns>
        public async Task<IMeeting> GetMeeting()
        {
            // Load up the normalized data.

            var al = new AgendaLoader(IndicoDataFetcher.Fetcher);
            var apiInfo = IndicoApiKeyAccess.GetKey(_info.AgendaSite);
            var agenda = await al.GetNormalizedConferenceData(_info, apiInfo == null ? null : apiInfo.ApiKey, apiInfo == null ? null : apiInfo.SecretKey);

            // Clean it up
            CleanUpMeeting(agenda);

            return new IndicoMeeting(agenda, _info.AsShortString());
        }

        /// <summary>
        /// Clean up the meeting (null talks, etc.)
        /// </summary>
        /// <param name="agenda"></param>
        private void CleanUpMeeting(Meeting agenda)
        {
            agenda.MeetingTalks = agenda.MeetingTalks.Where(m => m.StartDate.Year != 1 && m.EndDate.Year != 1).ToArray();
            foreach (var s in agenda.Sessions)
            {
                s.Talks = s.Talks.Where(t => t.StartDate.Year != 1 && t.EndDate.Year != 1).ToArray();
            }
            agenda.Sessions = agenda.Sessions.Where(s => s.StartDate.Year != 1 && s.EndDate.Year != 1).ToArray();
        }

        /// <summary>
        /// Return a short string that will represent this URL.
        /// </summary>
        /// <returns></returns>
        public string AsReferenceString()
        {
            return _info.AsShortString();
        }

        /// <summary>
        /// Return true if this is a valid meeting reference
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static bool IsValid(string url)
        {
            return AgendaInfo.IsValid(url);
        }

        /// <summary>
        /// Return the URL of the conference that can be opened in a web browser.
        /// </summary>
        public string WebURL
        {
            get { return _info.ConferenceUrl; }
        }
    }
}
