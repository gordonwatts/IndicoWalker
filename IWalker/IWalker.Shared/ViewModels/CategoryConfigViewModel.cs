using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using IWalker.DataModel.Categories;
using IWalker.DataModel.Interfaces;
using System.Reactive.Subjects;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Handles the config for a category (title, subscribed, displayed on main page, etc.)
    /// </summary>
    public class CategoryConfigViewModel : ReactiveObject
    {
        /// <summary>
        /// Get/Set if this particular feed is subscribed to.
        /// </summary>
        /// <remarks>This means that we will track it</remarks>
        public bool IsSubscribed
        {
            get { return _isSubscribed; }
            set { this.RaiseAndSetIfChanged(ref _isSubscribed, value); }
        }
        private bool _isSubscribed;

        /// <summary>
        /// Get/Set if this feed is displayed on the main page or not.
        /// </summary>
        public bool IsDisplayedOnMainPage
        {
            get { return _isDisplayedOnMainPage; }
            set { this.RaiseAndSetIfChanged(ref _isDisplayedOnMainPage, value); }
        }
        private bool _isDisplayedOnMainPage;

        /// <summary>
        /// Get/Set the title for this feed
        /// </summary>
        public string CategoryTitle
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        private string _title;

        /// <summary>
        /// Hold onto the meeting info for this meeting.
        /// </summary>
        private CategoryConfigInfo _meetingInfo = null;

        /// <summary>
        /// Each time we alter it, we spit something out on this. A null means it isn't subscribed.
        /// </summary>
        public IObservable<CategoryConfigInfo> UpdateToCI { get; private set; }
        private Subject<CategoryConfigInfo> _updateToCI;

        public CategoryConfigViewModel(CategoryConfigInfo ci)
        {
            _meetingInfo = ci;
            InitializeVM(null);
        }

        /// <summary>
        /// Initialize the settings interface for a particular category.
        /// </summary>
        public CategoryConfigViewModel(IMeetingListRef meeting)
        {
            // First, we need to determine if this meeting is already in the
            // database.

            _meetingInfo = CategoryDB.Find(meeting);
            InitializeVM(meeting);
        }

        /// <summary>
        /// Get everything else up and configured.
        /// </summary>
        /// <param name="meeting"></param>
        private void InitializeVM(IMeetingListRef meeting)
        {
            _isSubscribed = _meetingInfo != null;
            if (_meetingInfo == null)
            {
                _meetingInfo = new CategoryConfigInfo()
                {
                    MeetingList = meeting,
                    CategoryTitle = "Meeting List",
                    DisplayOnHomePage = false
                };
            }

            _title = _meetingInfo.CategoryTitle;

            // If they want it to be displayed on the main page, then we have to subscribe to it.

            this.WhenAny(x => x.IsDisplayedOnMainPage, x => x.Value)
                .Where(isDisplayedValue => isDisplayedValue)
                .Subscribe(v => IsSubscribed = true);

            // If they don't want to subscribe, then we can't display it.
            _isDisplayedOnMainPage = _meetingInfo.DisplayOnHomePage;
            this.WhenAny(x => x.IsSubscribed, x => x.Value)
                .Where(isSubscribed => !isSubscribed)
                .Subscribe(x => IsDisplayedOnMainPage = false);

            // When things change, we need to reflect the changes back into the main store.
            this.WhenAny(x => x.IsSubscribed, x => x.GetValue())
                .Where(x => x)
                .Subscribe(_ => CategoryDB.UpdateOrInsert(GetMeetingInfo()));
            this.WhenAny(x => x.IsSubscribed, x => x.GetValue())
                .Where(x => !x)
                .Subscribe(_ => CategoryDB.Remove(GetMeetingInfo()));

            this.WhenAny(x => x.IsDisplayedOnMainPage, x => x.GetValue())
                .Where(_ => IsSubscribed)
                .Subscribe(x => CategoryDB.UpdateOrInsert(GetMeetingInfo()));

            this.WhenAny(x => x.CategoryTitle, x => x.GetValue())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(_ => IsSubscribed)
                .Subscribe(x => CategoryDB.UpdateOrInsert(GetMeetingInfo()));

            // Setup the logic for subscribing (or not).

            _updateToCI = new Subject<CategoryConfigInfo>();
            UpdateToCI = _updateToCI;
        }

        /// <summary>
        /// Update the meeting info with the most recent stuff.
        /// </summary>
        /// <returns></returns>
        private CategoryConfigInfo GetMeetingInfo()
        {
            _meetingInfo.CategoryTitle = CategoryTitle;
            _meetingInfo.DisplayOnHomePage = IsDisplayedOnMainPage;
            return _meetingInfo;
        }
    }
}
