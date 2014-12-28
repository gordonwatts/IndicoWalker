using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Windows;
using IWalker.Util;
using IWalker.DataModel.Interfaces;
using IWalker.DataModel.Inidco;

namespace IWalker
{
    /// <summary>
    /// Main page with a simple button on it.
    /// </summary>
    public class StartPageViewModel :  ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// When clicked, it will cause the page to switch and the text to be saved.
        /// </summary>

        public ReactiveCommand<object> SwitchPages { get; set; }

        /// <summary>
        /// The meeting address (bindable).
        /// </summary>
        public string MeetingAddress
        {
            get { return _meetingAddress; }
            set { this.RaiseAndSetIfChanged(ref _meetingAddress, value); }
        }
        private string _meetingAddress;

        /// <summary>
        /// Setup the page
        /// </summary>
        public StartPageViewModel(IScreen screen)
        {
            HostScreen = screen;

            // We can switch pages only when the user has written something into the meeting address text.
            var canNavagateAway = this.WhenAny(x => x.MeetingAddress, x => !string.IsNullOrWhiteSpace(x.Value));
            SwitchPages = ReactiveCommand.Create(canNavagateAway);

            // When we navigate away, we should save the text and go
            SwitchPages
                .Select(x => MeetingAddress)
                .Subscribe(addr =>
                {
                    Settings.LastViewedMeeting = addr;
                    HostScreen.Router.Navigate.Execute(new MeetingPageViewModel(HostScreen, ConvertToIMeeting(addr)));
                });

            // Setup the first value for the last time we ran.
            MeetingAddress = Settings.LastViewedMeeting;
        }

        private IMeetingRef ConvertToIMeeting(string addr)
        {
            return new IndicoMeetingRef(addr);
        }

        /// <summary>
        /// Track the home screen.
        /// </summary>
        public IScreen HostScreen {get; private set;}

        /// <summary>
        /// Where we will be located.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/home"; }
        }
    }
}
