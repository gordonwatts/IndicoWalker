using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.ViewModels
{
    /// <summary>
    /// ViewModel for a category view - lists all the category items
    /// </summary>
    public class CategoryPageViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Initialize a new category page view model
        /// </summary>
        /// <param name="parent"></param>
        public CategoryPageViewModel(IScreen parent, IMeetingListRef meetings)
        {
            HostScreen = parent;
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
