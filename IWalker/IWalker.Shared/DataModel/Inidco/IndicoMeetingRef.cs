using IndicoInterface.NET;
using IndicoInterface.NET.SimpleAgendaDataModel;
using IWalker.DataModel.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

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
                Key = string.Format("{0}/{1}/{2}", meetingUniqueID, t.ID, t.SlideURL);
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
            /// Return the file associated with this talk.
            /// </summary>
            [JsonIgnore]
            public IFile TalkFile
            {
                get
                {
                    if (_file == null)
                    {
                        _file = new IndicoFile(aTalk.SlideURL, Key);
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
        }

        /// <summary>
        /// Represents a file on the indico server.
        /// </summary>
        public class IndicoFile : IFile
        {
            /// <summary>
            /// Get/Set the URL for the file.
            /// </summary>
            public Uri _aUrl { get; set; }

            /// <summary>
            /// Initialize with the URL for this talk
            /// </summary>
            /// <param name="fileUri"></param>
            public IndicoFile(string fileUri, string uniqueKey)
            {
                _aUrl = string.IsNullOrWhiteSpace(fileUri) ? null : new Uri(fileUri);
                UniqueKey = uniqueKey;
            }

            /// <summary>
            /// Does this object have any hope of fetching a file?
            /// </summary>
            [JsonIgnore]
            public bool IsValid
            {
                get { return _aUrl != null; }
            }

            /// <summary>
            /// Return a stream that can be used to read over the net.
            /// </summary>
            /// <returns></returns>
            public async Task<StreamReader> GetFileStream()
            {
                // Get the file, save it to the proper location, and then return it.
                Debug.WriteLine("  Doing download for {0}", _aUrl.OriginalString);
                return await IndicoDataFetcher.Fetcher.GetDataFromURL(_aUrl);
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
                    return Path.GetExtension(_aUrl.Segments.Last()).Substring(1);
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
                get { return Path.GetFileNameWithoutExtension(_aUrl.OriginalString); }
            }

            /// <summary>
            /// Given the URL, get the header info.
            /// </summary>
            /// <returns></returns>
            public async Task<string> GetFileDate()
            {
                var headers = await IndicoDataFetcher.Fetcher.GetContentHeadersFromUrl(_aUrl);
                if (!headers.LastModified.HasValue)
                    return "";
                return headers.LastModified.Value.ToString();
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
            var agenda = await al.GetNormalizedConferenceData(_info);
            return new IndicoMeeting(agenda, _info.AsShortString());
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
    }
}
