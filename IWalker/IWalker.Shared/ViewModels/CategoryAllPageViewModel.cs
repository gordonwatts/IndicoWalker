using IWalker.DataModel.Categories;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Represents a page that shows all the category items we know of and can look at.
    /// </summary>
    public class CategoryAllPageViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Represent the list of calendars
        /// </summary>
        public ReactiveList<CategoryConfigInfo> ListOfCalendars;

        /// <summary>
        /// Set us up with the complete list of calendars.
        /// </summary>
        /// <param name="parent"></param>
        public CategoryAllPageViewModel(IScreen parent)
        {
            HostScreen = parent;

            // The list of categories.
            ListOfCalendars = new ReactiveList<CategoryConfigInfo>();
            ListOfCalendars.AddRange(CategoryDB.LoadCategories());
        }

        /// <summary>
        /// Track the screen we are attached to for routing.
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// Return the path segment.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/CategoryAll"; }
        }
    }
}
