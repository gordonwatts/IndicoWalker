using CERNSSO;
using IWalker.DataModel.Inidco;
using IWalker.DataModel.Interfaces;
using IWalker.DataModel.MRU;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace IWalker.ViewModels
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
        /// The list of recently set meetings.
        /// </summary>
        public List<MRU> RecentMeetings { get; private set; }

        /// <summary>
        /// Reload from the DB all the meetings
        /// </summary>
        public ReactiveCommand<List<MRU>> LoadRecentMeetings { get; private set; }

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

            // Setup the first value for the last time we ran to make life a little simpler.
            MeetingAddress = Settings.LastViewedMeeting;

            // And populate the most recently viewed meeting list.
            RecentMeetings = new List<MRU>();

            LoadRecentMeetings = ReactiveCommand.CreateAsyncTask(async o =>
            {
                var m = new MRUDatabaseAccess();
                var list = 
                    (await m.QueryMRUDB())
                    .OrderByDescending(mru => mru.LastLookedAt)
                    .Take(20)
                    .OrderByDescending(mru => mru.StartTime)
                    .ToListAsync();
                return await list;
            });
            LoadRecentMeetings
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(l => {
                    RecentMeetings.Clear();
                    RecentMeetings.AddRange(l);
                });
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
