using IWalker.DataModel.Categories;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

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
        /// Return the current config view model
        /// </summary>
        public CategoryConfigViewModel ConfigViewModel
        {
            get { return _categoryConfig.Value; }
        }
        private ObservableAsPropertyHelper<CategoryConfigViewModel> _categoryConfig;

        /// <summary>
        /// The VM for the full listing of items in the category list.
        /// </summary>
        public CategoryURIViewModel CategoryFullListVM
        {
            get { return _catgoryFullListVM.Value; }
        }
        private ObservableAsPropertyHelper<CategoryURIViewModel> _catgoryFullListVM;

        /// <summary>
        /// True if a valid category has been selected.
        /// </summary>
        public bool ValidCategorySelected
        {
            get { return _validCategorySelected.Value; }
        }
        private ObservableAsPropertyHelper<bool> _validCategorySelected;

        /// <summary>
        /// Fire this with the CategoryConfigInfo of what should be shown in the
        /// details.
        /// </summary>
        public ReactiveCommand<object> ShowCategoryDetails { get; private set; }

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

            // And setup the category VM
            ShowCategoryDetails = ReactiveCommand.Create();
            var asCategoryInfo = ShowCategoryDetails
                .Cast <CategoryConfigInfo>();

            asCategoryInfo
                .Select(ci => new CategoryConfigViewModel(ci))
                .ToProperty(this, x => x.ConfigViewModel, out _categoryConfig, null);

            asCategoryInfo
                .Select(ci => new CategoryURIViewModel(ci.MeetingList))
                .ToProperty(this, x => x.CategoryFullListVM, out _catgoryFullListVM, null);

            // Keep the display "clean" until somethign is selected.
            asCategoryInfo
                .Select(_ => true)
                .ToProperty(this, x => x.ValidCategorySelected, out _validCategorySelected, false);
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
