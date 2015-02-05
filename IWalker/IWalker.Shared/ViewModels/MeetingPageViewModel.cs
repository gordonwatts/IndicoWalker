using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace IWalker.ViewModels
{
    /// <summary>
    /// A single meeting. A meeting has a single session, a conference has multiple sessions.
    /// </summary>
    public class MeetingPageViewModel : ReactiveObject, IRoutableViewModel
    {
        public MeetingPageViewModel(IScreen hs, IMeetingRef mRef)
        {
            // Initial default values
            HostScreen = hs;
            Talks = new ReactiveList<TalkUserControlViewModel>();

            // And start off a background guy to populate everything.
            LoadMeeting(mRef);
        }

        /// <summary>
        /// Given a meeting, load the info. Since this is an asynchronous command, we have to schedule stuff off it.
        /// </summary>
        /// <param name="title"></param>
        private void LoadMeeting(IMeetingRef title)
        {
            var ldrCmd = ReactiveCommand.CreateAsyncTask<IMeeting>(_ => title.GetMeeting());

            ldrCmd
                .Select(m => m.Title)
                .ToProperty(this, x => x.MeetingTitle, out _title, "");

            ldrCmd
                .Select(m => m.StartTime)
                .Select(dt => dt.ToString())
                .ToProperty(this, x => x.StartTime, out _startTime, "");

            ldrCmd
                .Select(m => m.Sessions)
                .Where(s => s != null && s.Length > 0)
                .Select(s => s[0])
                .Where(t => t.Talks != null)
                .Select(t => t.Talks)
                .Subscribe(talks => SetAsTalks(talks));

            // Start everything off.
            ldrCmd.Execute(null);
        }

        /// <summary>
        /// Given the talk list, make our current list look like it.
        /// </summary>
        /// <param name="talks"></param>
        private void SetAsTalks(ITalk[] talks)
        {
            Talks.AddRange(talks.Select(t => new TalkUserControlViewModel(t)));
        }

        /// <summary>
        /// Track the home screen.
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// The meeting title
        /// </summary>
        public string MeetingTitle
        {
            get { return _title.Value; }
        }
        private ObservableAsPropertyHelper<string> _title;

        /// <summary>
        /// The start time.
        /// </summary>
        public string StartTime
        {
            get { return _startTime.Value; }
        }
        private ObservableAsPropertyHelper<string> _startTime;

        /// <summary>
        /// Get the list of talks
        /// </summary>
        public ReactiveList<TalkUserControlViewModel> Talks { get; private set; }

        /// <summary>
        /// Where we will be located.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/meeting"; }
        }
    }
}
