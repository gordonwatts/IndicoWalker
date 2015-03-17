using IWalker.DataModel.Categories;
using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CategoryAllPageView : Page, IViewFor<CategoryAllPageViewModel>
    {
        public CategoryAllPageView()
        {
            this.InitializeComponent();
            this.OneWayBind(ViewModel, x => x.ListOfCalendars, y => y.CategoryNames.ItemsSource);
            this.ObservableForProperty(x => x.ViewModel)
                .Select(vm => vm.Value)
                .Where(vm => vm != null)
                .Subscribe(vm =>
                {
                    vm.ViewCategory
                        .Subscribe(nextCi => ViewModel.HostScreen.Router.Navigate.Execute(new CategoryPageViewModel(ViewModel.HostScreen, nextCi.MeetingList)));
                });
        }

        /// <summary>
        /// The viewmodel that backs this page
        /// </summary>
        public CategoryAllPageViewModel ViewModel
        {
            get { return (CategoryAllPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CategoryAllPageViewModel), typeof(CategoryAllPageView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CategoryAllPageViewModel)value; }
        }

        /// <summary>
        /// User has selected an item. Feed it back for checking and eventual selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CategoryNames_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ShowCategoryDetails.Execute(e.ClickedItem as CategoryConfigInfo);
        }
    }
}
