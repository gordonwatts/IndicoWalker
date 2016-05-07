using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace IWalker.ViewModels
{
    /// <summary>
    /// A user control that shows all the data for a single category URI.
    /// </summary>
    /// <remarks>Split out like this as we may want to host this display in several places</remarks>
    public class CategoryURIViewModel : ReactiveObject
    {
        /// <summary>
        /// Get the list of meetings that we are going to be looking at
        /// </summary>
        public ReactiveList<IMeetingRefExtended> MeetingList { get; private set; }

        /// <summary>
        /// Fire this to view a meeting
        /// </summary>
        public ReactiveCommand<object> ViewMeeting { get; private set; }

        /// <summary>
        /// Observe the meetings we should navagate away to
        /// </summary>
        public IObservable<MeetingPageViewModel> MeetingToVisit { get; private set; }

        /// <summary>
        /// View model for errors that we find.
        /// </summary>
        public ErrorUserControlViewModel ErrorsVM { get; private set; }

        /// <summary>
        /// Readonly true if the category list is ready to display.
        /// </summary>
        public bool IsReady
        {
            get { return _isReady.Value; }
        }
        private ObservableAsPropertyHelper<bool> _isReady;

        /// <summary>
        /// Init ourselves with a new meeting ref
        /// </summary>
        /// <param name="meetings"></param>
        public CategoryURIViewModel(IMeetingListRef meetings, IBlobCache cache = null)
        {
            cache = cache ?? Blobs.LocalStorage;

            // Get the list of items we are going to show. If there
            // is an error we should display it.
            MeetingList = new ReactiveList<IMeetingRefExtended>();
            var meetingStream = meetings.FetchAndUpdateRecentMeetings(cache: cache)
                .Replay(1);

            meetingStream
                .OnErrorResumeNext(Observable.Empty<IMeetingRefExtended[]>())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(m => SetMeetings(m));

            var errorStream = new Subject<Exception>();
            ErrorsVM = new ErrorUserControlViewModel(errorStream);

            meetingStream
                .Subscribe(_ => { },
                except => errorStream.OnNext(except));

            meetingStream
                .Select(_ => true)
                .ToProperty(this, x => x.IsReady, out _isReady, false, RxApp.MainThreadScheduler);

            meetingStream.Connect();

            // When the user wants to view one of the meetings we are showing
            ViewMeeting = ReactiveCommand.Create();
            MeetingToVisit = ViewMeeting
                .Select(m => m as IMeetingRefExtended)
                .Where(m => m != null)
                .Select(m => new MeetingPageViewModel(Locator.Current.GetService<IScreen>(), m.Meeting));
        }

        /// <summary>
        /// Set the meeting list as desired. Do our best to be "neat" about it.
        /// </summary>
        /// <param name="m"></param>
        private void SetMeetings(IMeetingRefExtended[] m)
        {
            // We need the list to be sorted by date
            var sortedMeetings = m.OrderBy(s => s.StartTime);

            MeetingList.MakeListLookLike(sortedMeetings,
                (oItem, dItem) => oItem.Meeting.AsReferenceString() == dItem.Meeting.AsReferenceString(),
                dItem => dItem
                );
        }

    }
}
