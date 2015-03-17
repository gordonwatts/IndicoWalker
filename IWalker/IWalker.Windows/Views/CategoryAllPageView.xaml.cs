using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

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
            this.BindCommand(ViewModel, x => x.HostScreen.Router.NavigateBack, y => y.backButton);
            this.OneWayBind(ViewModel, x => x.ListOfCalendars, y => y.CategoryNames.ItemsSource);
            this.Bind(ViewModel, x => x.ConfigViewModel, y => y.CatConfig.ViewModel);
            this.Bind(ViewModel, x => x.CategoryFullListVM, y => y.CatListing.ViewModel);
            this.OneWayBind(ViewModel, x => x.ValidCategorySelected, y => y.DetailsGrid.Visibility);
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
        /// Ok - we have the next item to look at!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CategoryNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.ShowCategoryDetails.Execute(e.AddedItems[0]);
        }
    }
}
