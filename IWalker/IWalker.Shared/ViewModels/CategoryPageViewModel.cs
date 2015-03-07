using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Text;
using Akavache;

namespace IWalker.ViewModels
{
    /// <summary>
    /// ViewModel for a category view - lists all the category items
    /// </summary>
    public class CategoryPageViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Get the list of meetings that we are going to be looking at
        /// </summary>
        public ReactiveList<IMeetingRefExtended> MeetingList { get; private set; }

        /// <summary>
        /// Initialize a new category page view model
        /// </summary>
        /// <param name="parent"></param>
        public CategoryPageViewModel(IScreen parent, IMeetingListRef meetings)
        {
            HostScreen = parent;

            // Get the list of items we are going to show.
            MeetingList = new ReactiveList<IMeetingRefExtended>();
            Blobs.LocalStorage.GetAndFetchLatest(meetings.UniqueString, async () => (await meetings.GetMeetings(60)).ToArray(), null)
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
                (oItem, dItem) => oItem.Equals(dItem),
                dItem => dItem
                );
        }

        /// <summary>
        /// Return the host screen.
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// Return a URL pointer
        /// </summary>
        public string UrlPathSegment { get { return "/Category"; } }
    }
}
