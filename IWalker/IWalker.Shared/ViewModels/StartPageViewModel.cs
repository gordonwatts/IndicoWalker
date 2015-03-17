using CERNSSO;
using IndicoInterface.NET;
using IWalker.DataModel.Inidco;
using IWalker.DataModel.Interfaces;
using IWalker.DataModel.MRU;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using IWalker.DataModel.Categories;
using Windows.UI.Popups;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Main page with a simple button on it.
    /// </summary>
    public class StartPageViewModel :  ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// When clicked, it will open the requested meeting in a new xaml page.
        /// </summary>
        public ReactiveCommand<object> SwitchPages { get; set; }

        /// <summary>
        /// Pass in a MRU object to have it opened in a new xaml page.
        /// </summary>
        public ReactiveCommand<object> OpenMRUMeeting { get; private set; }

        /// <summary>
        /// Pass in an upcoming meeting to be opened.
        /// </summary>
        public ReactiveCommand<object> OpenUpcomingMeeting { get; private set; }

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
        public ReactiveList<MRU> RecentMeetings { get; private set; }

        /// <summary>
        /// The list of meetings that are up coming
        /// </summary>
        public ReactiveList<IMeetingRefExtended> UpcomingMeetings { get; private set; }

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

            // MRU button was pressed.
            OpenMRUMeeting = ReactiveCommand.Create();
            OpenMRUMeeting
                .Cast<MRU>()
                .Select(mru => ConvertToIMeeting(mru))
                .Subscribe(addr => HostScreen.Router.Navigate.Execute(new MeetingPageViewModel(HostScreen, addr)));

            // And an upcoming meeting was pushed...
            OpenUpcomingMeeting = ReactiveCommand.Create();
            OpenUpcomingMeeting
                .Cast<IMeetingRefExtended>()
                .Where(m => m != null)
                .Subscribe(m => HostScreen.Router.Navigate.Execute(new MeetingPageViewModel(HostScreen, m.Meeting)));

            // Setup the first value for the last time we ran to make life a little simpler.
            MeetingAddress = Settings.LastViewedMeeting;

            // And populate the most recently viewed meeting list.
            RecentMeetings = new ReactiveList<MRU>();

            LoadRecentMeetings = ReactiveCommand.CreateAsyncTask(async o =>
            {
                var m = new MRUDatabaseAccess();
                var mruMeetings = 
                    (await m.QueryMRUDB())
                    .OrderByDescending(mru => mru.LastLookedAt)
                    .Take(20)
                    .ToListAsync();
                return (await mruMeetings)
                    .OrderByDescending(mru => mru.StartTime)
                    .ToList();
            });
            LoadRecentMeetings
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(l => {
                    RecentMeetings.Clear();
                    RecentMeetings.AddRange(l);
                });

            // Upcoming meetings. This is easy - we fetch once.
            // But since they are coming from multiple sources, we have to be a little
            // careful about combining them.
            UpcomingMeetings = new ReactiveList<IMeetingRefExtended>();
            var updateUpcomingMeetings = ReactiveCommand.Create();
            var meetingList = from xup in updateUpcomingMeetings
                    from category in CategoryDB.LoadCategories()
                    where category.DisplayOnHomePage
                    from meetings in category.MeetingList.FetchAndUpdateRecentMeetings(false)
                    select Tuple.Create(category.MeetingList, meetings);

            meetingList
                .Select(ml => {
                    _meetingCatalog[ml.Item1.UniqueString] = ml.Item2;
                    return _meetingCatalog;
                })
                .Select(mc => mc.SelectMany(mi => mi.Value).Where(mi => mi.StartTime.Within(TimeSpan.FromDays(Settings.DaysOfUpcomingMeetingsToShowOnMainPage))).OrderByDescending(minfo => minfo.StartTime))
                .Subscribe(meetings => SetUpcomingMeetings(meetings));

            updateUpcomingMeetings.Execute(null);
        }

        /// <summary>
        /// Set/update the upcoming meetings
        /// </summary>
        /// <param name="meetings">List of the upcoming meeting</param>
        /// <returns></returns>
        private void SetUpcomingMeetings(IEnumerable<IMeetingRefExtended> meetings)
        {
            UpcomingMeetings.MakeListLookLike(meetings,
                (oItem, dItem) => oItem.Meeting.AsReferenceString() == dItem.Meeting.AsReferenceString(),
                dItem => dItem
                );
        }

        /// <summary>
        /// Keep track of all the various meetings, and how we are going to have to combine them.
        /// </summary>
        Dictionary<string, IMeetingRefExtended[]> _meetingCatalog = new Dictionary<string, IMeetingRefExtended[]>();

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
        /// Given a MRU, convert it to a meeting.
        /// </summary>
        /// <param name="mru">The MRU.</param>
        /// <returns></returns>
        private IMeetingRef ConvertToIMeeting(MRU mru)
        {
            var ag = AgendaInfo.FromShortString(mru.IDRef);
            return new IndicoMeetingRef(ag);
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
