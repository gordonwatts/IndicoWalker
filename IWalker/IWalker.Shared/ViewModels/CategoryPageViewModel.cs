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
        public CategoryURIViewModel CategoryListing { get; private set; }

        public CategoryConfigViewModel CategoryConfig { get; private set; }

        /// <summary>
        /// Initialize a new category page view model
        /// </summary>
        /// <param name="parent"></param>
        public CategoryPageViewModel(IScreen parent, IMeetingListRef meetings)
        {
            HostScreen = parent;
            CategoryListing = new CategoryURIViewModel(meetings);
            CategoryConfig = new CategoryConfigViewModel();
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
