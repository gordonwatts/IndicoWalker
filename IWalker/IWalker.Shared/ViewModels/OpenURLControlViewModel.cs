using IWalker.Util;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using IWalker.DataModel.Inidco;
using Windows.UI.Popups;
using IWalker.DataModel.Interfaces;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Enable the user to enter a URL and then open either a category or an
    /// indico meeting from the URL that he user enters.
    /// </summary>
    public class OpenURLControlViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// When clicked, it will open the requested meeting in a new xaml page.
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
        /// Initialize the URL opener.
        /// </summary>
        /// <param name="screen"></param>
        public OpenURLControlViewModel(IScreen screen)
        {
            HostScreen = screen;
            
            // Setup the first value for the last time we ran to make life a little simpler.
            MeetingAddress = Settings.LastViewedMeeting;

            // We can switch pages only when the user has written something into the meeting address text.
            var canNavagateAway = this.WhenAny(x => x.MeetingAddress, x => !string.IsNullOrWhiteSpace(x.Value));
            SwitchPages = ReactiveCommand.Create(canNavagateAway);

            // When we navigate away, we should save the text and go
            SwitchPages
                .Select(x => MeetingAddress)
                .Where(x => IsMeeting(x))
                .Subscribe(addr =>
                {
                    Settings.LastViewedMeeting = addr;
                    HostScreen.Router.Navigate.Execute(new MeetingPageViewModel(HostScreen, ConvertToIMeeting(addr)));
                });
            SwitchPages
                .Select(x => MeetingAddress)
                .Where(x => IsAgendaList(x))
                .Subscribe(addr =>
                {
                    Settings.LastViewedMeeting = addr;
                    HostScreen.Router.Navigate.Execute(new CategoryPageViewModel(HostScreen, ConvertToIAgendaList(addr)));
                });

            // Finally, if we don't know what to do with it, we come here.
            SwitchPages
                .Select(x => MeetingAddress)
                .Where(x => !IsMeeting(x) && !IsAgendaList(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => new MessageDialog("That is not something I recognize as a meeting address or a meeting category address!"))
                .SelectMany(d => d.ShowAsync())
                .Subscribe();

        }

        /// <summary>
        /// The URL for stashing it on the navagation stack
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/LoadMeeting"; }
        }

        /// <summary>
        /// How to get to navagation, etc.
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// Return a list of possible meetings
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private IMeetingListRef ConvertToIAgendaList(string addr)
        {
            return new IndicoMeetingListRef(addr);
        }

        /// <summary>
        /// Convert text entry to some sort of address.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private IMeetingRef ConvertToIMeeting(string addr)
        {
            return new IndicoMeetingRef(addr);
        }

        /// <summary>
        /// Return true if this is url is a valid agenda listing
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool IsAgendaList(string url)
        {
            return IndicoMeetingListRef.IsValid(url);
        }

        /// <summary>
        /// Return true if this is a meeting
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private bool IsMeeting(string url)
        {
            return IndicoMeetingRef.IsValid(url);
        }

    }
}
