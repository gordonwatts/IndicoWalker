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
            public IndicoSesson(Session s)
            {
                // TODO: Complete member initialization
                this.aSession = s;
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
                        _talks = aSession.Talks.Select(t => new IndicoTalk(t)).ToArray();
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
            /// <summary>
            /// Get/Set the indico talk that this is associated with. Don't touch - for the serializer.
            /// </summary>
            public Talk aTalk { get; set; }

            public IndicoTalk(Talk t)
            {
                // TODO: Complete member initialization
                this.aTalk = t;
            }

            /// <summary>
            /// Get the talk title.
            /// </summary>
            [JsonIgnore]
            public string Title
            {
                get { return aTalk.Title; }
            }

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
                        _file = new IndicoFile(aTalk.SlideURL);
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
            /// <summary>
            /// Get/Set the url for the file.
            /// </summary>
            public Uri aUrl { get; set; }

            /// <summary>
            /// Initialize with the URL for this talk
            /// </summary>
            /// <param name="fileUri"></param>
            public IndicoFile(string fileUri)
            {
                aUrl = string.IsNullOrWhiteSpace(fileUri) ? null : new Uri(fileUri);
            }

            /// <summary>
            /// Does this object have any hope of fetching a file?
            /// </summary>
            [JsonIgnore]
            public bool IsValid
            {
                get { return aUrl != null; }
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
                Debug.WriteLine("Entering DownloadFile {0}", aUrl.OriginalString);

                // Get the file resetting place for the file name
                var fname = CleanFilename(aUrl.AbsolutePath);

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
                    Debug.WriteLine("  File already downloaded for {0}", aUrl.OriginalString);
                    return file;
                } catch {

                }

                // Get the file, save it to the proper location, and then return it.
                Debug.WriteLine("  Doing download for {0}", aUrl.OriginalString);
                var dataStream = await _fetcher.Value.GetDataFromURL(aUrl);
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
                Debug.WriteLine("  Finished download of {0}", aUrl.OriginalString);
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

                // Get the file resetting place for the file name
                var fname = CleanFilename(aUrl.AbsolutePath);
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
            /// Return the file type based on the URL.
            /// </summary>
            [JsonIgnore]
            public string FileType
            {
                get {
                    if (!IsValid)
                        return "";
                    return Path.GetExtension(aUrl.Segments.Last()).Substring(1);
                }
            }
        }


        /// <summary>
        /// The meeting
        /// </summary>
        internal class IndicoMeeting : IMeeting
        {
            /// <summary>
            /// Hold onto a complete agenda internally.
            /// </summary>
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
            /// Get the sessions. Populate them if need be!
            /// </summary>
            [JsonIgnore]
            public ISession[] Sessions
            {
                get
                {
                    if (_sessons == null)
                    {
                        _sessons = aAgenda.Sessions.Select(s => new IndicoSesson(s)).ToArray();
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
