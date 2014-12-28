using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker
{
    /// <summary>
    /// A single meeting
    /// </summary>
    public class MeetingPageViewModel : ReactiveObject, IRoutableViewModel
    {
        public MeetingPageViewModel(IScreen hs, IMeetingRef mRef)
        {
            // Initial default values
            HostScreen = hs;
            MeetingTitle = "";

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

            ldrCmd.Subscribe(x => { MeetingTitle = x.Title; });
            ldrCmd.Execute(null);
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
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        private string _title;

        /// <summary>
        /// Where we will be located.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/meeting"; }
        }
    }
}
