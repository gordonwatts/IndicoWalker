﻿using IndicoInterface.NET;
using IndicoInterface.NET.SimpleAgendaDataModel;
using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

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
        private string _url;

        /// <summary>
        /// Initialize with a URL
        /// </summary>
        /// <param name="url"></param>
        public IndicoMeetingRef(string url)
        {
            _url = url;
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
                // Get the file reseting place for the file name
                var fname = CleanFilename(_url.AbsolutePath);

                // Now, see if the file exists already. If so, we can just return it.
                var local = ApplicationData.Current.LocalFolder;
                var indico = (await local.TryGetItemAsync("indico")) as StorageFolder;
                if (indico == null)
                {
                    indico = await local.CreateFolderAsync("indico");
                }

                var file = (await indico.TryGetItemAsync(fname)) as StorageFile;
                if (file != null)
                    return file;

                // Get the file, save it to the proper location, and then return it.
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

            /// <summary>
            /// Start up and cache the meeting agenda.
            /// </summary>
            /// <param name="agenda"></param>
            public IndicoMeeting(Meeting agenda)
            {
                this._agenda = agenda;
            }

            public string Title
            {
                get { return _agenda.Title; }
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

            var a = new AgendaInfo(_url);
            var al = new AgendaLoader(_fetcher.Value);
            var agenda = await al.GetNormalizedConferenceData(a);
            return new IndicoMeeting(agenda);
        }
    }
}
