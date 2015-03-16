using IWalker.ViewModels;
using ReactiveUI;
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
    }
}
