using IndicoInterface.NET;
using IndicoInterface.NET.SimpleAgendaDataModel;
using IWalker.DataModel.Interfaces;
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
            private Session _session;

            /// <summary>
            /// Cache the session for later use.
            /// </summary>
            /// <param name="s"></param>
            public IndicoSesson(Session s)
            {
                // TODO: Complete member initialization
                this._session = s;
            }

            private IndicoTalk[] _talks = null;

            /// <summary>
            /// Get the talks for this meeting.
            /// </summary>
            public ITalk[] Talks
            {
                get
                {
                    if (_talks == null)
                    {
                        _talks = _session.Talks.Select(t => new IndicoTalk(t)).ToArray();
                    }
                    return _talks;
                }
            }
        }

        /// <summary>
        /// Implement the interface to a talk
        /// </summary>
        public class IndicoTalk : ITalk
        {
            private Talk _talk;

            public IndicoTalk(Talk t)
            {
                // TODO: Complete member initialization
                this._talk = t;
            }

            /// <summary>
            /// Get the URL for the slide
            /// </summary>
            //public string SlideURL
            //{
            //    get { return _talk.SlideURL; }
            //}

            /// <summary>
            /// Get the talk title.
            /// </summary>
            public string Title
            {
                get { return _talk.Title; }
            }

            private IndicoFile _file;

            /// <summary>
            /// Retrn the file associated with this talk.
            /// </summary>
            public IFile TalkFile
            {
                get
                {
                    if (_file == null)
                    {
                        _file = new IndicoFile(_talk.SlideURL);
                    }
                    return _file;
                }
            }
        }

        /// <summary>
        /// Represents a file on the indico server.
        /// </summary>
        public class IndicoFile : IFile
        {
            private Uri _url;

            /// <summary>
            /// Initialize with the url for this talk
            /// </summary>
            /// <param name="fileUri"></param>
            public IndicoFile(string fileUri)
            {
                _url = string.IsNullOrWhiteSpace(fileUri) ? null : new Uri(fileUri);
            }

            /// <summary>
            /// Does this object have any hope of fetching a file?
            /// </summary>
            public bool IsValid
            {
                get { return _url != null; }
            }

            /// <summary>
            /// Download the file from indico, and store it locally in some unique spot.
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// This will change as we move forward to just being a stream of some sort.
            /// </remarks>
            public async Task<StorageFile> DownloadFile()
            {
                Debug.Assert(IsValid);
                Debug.WriteLine("Entering DownloadFile {0}", _url.OriginalString);

                // Get the file reseting place for the file name
                var fname = CleanFilename(_url.AbsolutePath);

                // Now, see if the file exists already. If so, we can just return it.
                var local = ApplicationData.Current.LocalFolder;
                var indico = await local.CreateFolderAsync("indico", CreationCollisionOption.OpenIfExists);
                if (indico == null)
                {
                    indico = await local.CreateFolderAsync("indico");
                }

                StorageFile file = null;
                try {
                    file = await indico.GetFileAsync(fname);
                    Debug.WriteLine("  File already downloaded for {0}", _url.OriginalString);
                    return file;
                } catch {

                }

                // Get the file, save it to the proper location, and then return it.
                Debug.WriteLine("  Doing download for {0}", _url.OriginalString);
                var dataStream = await _fetcher.Value.GetDataFromURL(_url);
                var fnameTemp = fname + "-temp";
                var tempFile = await indico.CreateFileAsync(fnameTemp, CreationCollisionOption.ReplaceExisting);
                var outputStream = await tempFile.OpenAsync(FileAccessMode.ReadWrite);
                using (var sw = outputStream.AsStreamForWrite())
                {
                    await dataStream.BaseStream.CopyToAsync(sw);
                }

                await tempFile.RenameAsync(fname);

                // Finally, get back the file and return it.
                file = await indico.GetFileAsync(fname);
                Debug.WriteLine("  Finished download of {0}", _url.OriginalString);
                return file;
            }

            /// <summary>
            /// Clean up a string so it can be used as a filename.
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            private string CleanFilename(string str)
            {
                return str
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(":", "_")
                    .Replace("?", "_")
                    .Replace("=", "_")
                    .Replace("&", "_");
            }

            /// <summary>
            /// See if the file is currently local or not.
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// Will not create anything.
            /// This will be deleted when we have a real backing store as well.
            /// </remarks>
            public async Task<bool> IsLocal()
            {
                // Just cut off if it isn't valid.
                if (!IsValid)
                    return false;

                // Get the file reseting place for the file name
                var fname = CleanFilename(_url.AbsolutePath);
                var local = ApplicationData.Current.LocalFolder;
                try
                {
                    var indico = await local.GetFolderAsync("indico");
                    var file = await indico.GetFileAsync(fname);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Return the file type based on the url.
            /// </summary>
            public string FileType
            {
                get {
                    if (!IsValid)
                        return "";
                    return Path.GetExtension(_url.Segments.Last()).Substring(1);
                }
            }
        }


        /// <summary>
        /// The meeting
        /// </summary>
        private class IndicoMeeting : IMeeting
        {
            /// <summary>
            /// Hold onto a complete agenda internally.
            /// </summary>
            private Meeting _agenda;

            private IndicoSesson[] _sessons = null;

            private string _shortString;

            /// <summary>
            /// Start up and cache the meeting agenda.
            /// </summary>
            /// <param name="agenda"></param>
            public IndicoMeeting(Meeting agenda, string shortString)
            {
                this._agenda = agenda;
                _shortString = shortString;
            }

            public string Title
            {
                get { return _agenda.Title; }
            }

            /// <summary>
            /// Return the date of the meeting
            /// </summary>
            public DateTime StartTime
            {
                get { return _agenda.StartDate; }
            }

            /// <summary>
            /// Get the sessions. Populate them if need be!
            /// </summary>
            public ISession[] Sessions
            {
                get
                {
                    if (_sessons == null)
                    {
                        _sessons = _agenda.Sessions.Select(s => new IndicoSesson(s)).ToArray();
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
                return _shortString;
            }
        }

        /// <summary>
        /// Hold onto the fetcher singleton.
        /// </summary>
        static Lazy<IndicoDataFetcher> _fetcher = new Lazy<IndicoDataFetcher>(() => new IndicoDataFetcher());

        /// <summary>
        /// Get the meeting info for this Indico agenda.
        /// </summary>
        /// <returns></returns>
        public async Task<IMeeting> GetMeeting()
        {
            // Load up the normalized data.

            var al = new AgendaLoader(_fetcher.Value);
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
    }
}
