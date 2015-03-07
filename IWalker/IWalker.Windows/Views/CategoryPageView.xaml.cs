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
    public sealed partial class CategoryPageView : Page, IViewFor<CategoryPageViewModel>
    {
        public CategoryPageView()
        {
            this.InitializeComponent();
            this.BindCommand(ViewModel, x => x.HostScreen.Router.NavigateBack, y => y.backButton);
            this.OneWayBind(ViewModel, x => x.MeetingList, y => y.MeetingList.ItemsSource);
        }

        /// <summary>
        /// The viewmodel that backs this page
        /// </summary>
        public CategoryPageViewModel ViewModel
        {
            get { return (CategoryPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CategoryPageViewModel), typeof(CategoryPageView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CategoryPageViewModel)value; }
        }

    }
}
