using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;

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
        /// Init ourselves with a new meeting ref
        /// </summary>
        /// <param name="meetings"></param>
        public CategoryURIViewModel(IMeetingListRef meetings)
        {
            // Get the list of items we are going to show.
            MeetingList = new ReactiveList<IMeetingRefExtended>();
            meetings.FetchAndUpdateRecentMeetings()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(m => SetMeetings(m));
        }

        /// <summary>
        /// Set the meeting list as desired. Do our best to be "neat" about it.
        /// </summary>
        /// <param name="m"></param>
        private void SetMeetings(IMeetingRefExtended[] m)
        {
            MeetingList.MakeListLookLike(m,
                (oItem, dItem) => oItem.Meeting.AsReferenceString() == dItem.Meeting.AsReferenceString(),
                dItem => dItem
                );
        }

    }
}
