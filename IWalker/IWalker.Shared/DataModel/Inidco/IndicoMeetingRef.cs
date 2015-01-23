using IndicoInterface.NET;
using IndicoInterface.NET.SimpleAgendaDataModel;
using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

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
