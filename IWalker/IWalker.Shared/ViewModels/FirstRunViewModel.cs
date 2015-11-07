using IWalker.DataModel.Categories;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace IWalker.ViewModels
{
    /// <summary>
    /// If the user has never ever run this before, we display the view associated with this
    /// VM, which gives them a chance to add default items (e.g. conferences).
    /// </summary>
    public class FirstRunViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// When run, we will add the default categories to the list, download them,
        /// and then move on.
        /// </summary>
        public ReactiveCommand<object> AddDefaultCategories { get; private set; }

        /// <summary>
        /// Move directly onto the home screen without adding and fetching defualt
        /// categories.
        /// </summary>
        public ReactiveCommand<object> SkipDefaultCategories { get; private set; }

        /// <summary>
        /// Set to the title of the thing we are trying to fetch
        /// </summary>
        public string ItemBeingFetched
        {
            get { return _itemBeingFetched; }
            private set { this.RaiseAndSetIfChanged(ref _itemBeingFetched, value); }
        }
        private string _itemBeingFetched;

        /// <summary>
        /// True if we are updating the category items
        /// </summary>
        public bool FetchingItems
        {
            get { return _fetchingItems; }
            private set { this.RaiseAndSetIfChanged(ref _fetchingItems, value); }
        }
        private bool _fetchingItems;

        /// <summary>
        /// Path segment for the stack.
        /// </summary>
        public string UrlPathSegment { get { return "/firstrun"; } }

        /// <summary>
        /// The screen we are attached to
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// A list of default items we are going after.
        /// </summary>
        private Tuple<string, string>[] _defaultItems =
        {
            Tuple.Create("Argonne Lab: HEP Conferences", "https://indico.hep.anl.gov/indico/categoryDisplay.py?categId=2"),
            Tuple.Create("Argonne Lab: Future pp Colliders", "https://indico.hep.anl.gov/indico/categoryDisplay.py?categId=26"),
        };

        /// <summary>
        /// Configure the VM to add items if the user requests.
        /// </summary>
        /// <param name="screen"></param>
        public FirstRunViewModel(IScreen screen)
        {
            HostScreen = screen;

            // Init variables everyone will be looking at.
            _fetchingItems = false;
            _itemBeingFetched = "";

            // Adding them means putting them in our DB and
            // also doing the query (so the user lands with something
            // interesting in their input feed).

            AddDefaultCategories = ReactiveCommand.Create();

            Exception bummer = null;
            var categoryItems = AddDefaultCategories
                .Take(1)
                .SelectMany(_ => _defaultItems)
                .Select(item => makeAMeeting(item.Item1, item.Item2))
                .Select(cat =>
                {
                    FetchingItems = true;
                    ItemBeingFetched = cat.CategoryTitle;
                    CategoryDB.SaveCategories(CategoryDB.LoadCategories().Concat(new CategoryConfigInfo[] { cat }).ToList());
                    return cat;
                })
                .SelectMany(cat => cat.MeetingList.FetchAndUpdateRecentMeetings(cache: Blobs.LocalStorage))
                .Catch<IMeetingRefExtended[], Exception>(e =>
                {
                    bummer = e;
                    return Observable.Empty<IMeetingRefExtended[]>();
                })
                .Finally(() => new int[] { 1 }.ToObservable().ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => HostScreen.Router.Navigate.Execute(new StartPageViewModel(HostScreen))))
                .Subscribe();

            // When we want to skip the intro, we just move onto the main
            // page. This is a pretty easy thing of it.

            SkipDefaultCategories = ReactiveCommand.Create();
            SkipDefaultCategories
                .Subscribe(_ => HostScreen.Router.Navigate.Execute(new StartPageViewModel(HostScreen)));

        }

        /// <summary>
        /// Helper to generate our meetings.
        /// </summary>
        private IMeetingListRefFactory _generateMeetings = null;

        /// <summary>
        /// Create a meeting reference.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <remarks>Currently can only deal with indico meetings</remarks>
        private CategoryConfigInfo makeAMeeting(string title, string url)
        {
            if (_generateMeetings == null)
            {
                _generateMeetings = Locator.Current.GetService<IMeetingListRefFactory>();
            }
            return new CategoryConfigInfo() { CategoryTitle = title, DisplayOnHomePage = true, MeetingList = _generateMeetings.GenerateMeetingListRef(url) };
        }
    }
}
