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
using IWalker.DataModel.Categories;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Main page with a simple button on it.
    /// </summary>
    public class StartPageViewModel :  ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Pass in a MRU object to have it opened in a new xaml page.
        /// </summary>
        public ReactiveCommand<object> OpenMRUMeeting { get; private set; }

        /// <summary>
        /// Pass in an upcoming meeting to be opened.
        /// </summary>
        public ReactiveCommand<object> OpenUpcomingMeeting { get; private set; }

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
        /// Reload the up coming meetings
        /// </summary>
        public ReactiveCommand<object> UpdateUpcomingMeetings { get; private set; }

        /// <summary>
        /// Attache the Open URL Control VM.
        /// </summary>
        public OpenURLControlViewModel OpenURLControlVM { get; private set; }

        /// <summary>
        /// Setup the page
        /// </summary>
        public StartPageViewModel(IScreen screen)
        {
            HostScreen = screen;

            // A Open URL Control View
            OpenURLControlVM = new OpenURLControlViewModel(screen);

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
                .Subscribe(l => SetMRUMeetings(l));

            // Upcoming meetings. This is easy - we fetch once.
            // But since they are coming from multiple sources, we have to be a little
            // careful about combining them.
            UpcomingMeetings = new ReactiveList<IMeetingRefExtended>();
            UpdateUpcomingMeetings = ReactiveCommand.Create();
            var meetingList = from xup in UpdateUpcomingMeetings
                    from category in CategoryDB.LoadCategories()
                              from meetings in (category.DisplayOnHomePage ? category.MeetingList.FetchAndUpdateRecentMeetings(false).OnErrorResumeNext(Observable.Empty<IMeetingRefExtended[]>()) : Observable.Return(new IMeetingRefExtended[0]))
                    select Tuple.Create(category.MeetingList, meetings);

            meetingList
                .Select(ml => {
                    _meetingCatalog[ml.Item1.UniqueString] = ml.Item2;
                    return _meetingCatalog;
                })
                .Select(mc => mc.SelectMany(mi => mi.Value).Where(mi => mi.StartTime.Within(TimeSpan.FromDays(Settings.DaysOfUpcomingMeetingsToShowOnMainPage))).OrderByDescending(minfo => minfo.StartTime).ToArray())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(meetings => SetUpcomingMeetings(meetings));

            UpdateUpcomingMeetings.Execute(null);
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
        /// Set/Update teh MRU meeting list
        /// </summary>
        /// <param name="meetings"></param>
        private void SetMRUMeetings(IEnumerable<MRU> meetings)
        {
            RecentMeetings.MakeListLookLike(meetings,
                (oItem, dItem) => oItem.IDRef == dItem.IDRef && oItem.StartTime == dItem.StartTime && oItem.Title == dItem.Title,
                dItem => dItem
                );
        }

        /// <summary>
        /// Keep track of all the various meetings, and how we are going to have to combine them.
        /// </summary>
        Dictionary<string, IMeetingRefExtended[]> _meetingCatalog = new Dictionary<string, IMeetingRefExtended[]>();

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
