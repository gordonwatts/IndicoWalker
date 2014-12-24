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
        public MeetingPageViewModel(IScreen hs, string title)
        {
            HostScreen = hs;
            MeetingTitle = title;
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
