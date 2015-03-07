using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class CategoryURIUserControl : UserControl, IViewFor<CategoryURIViewModel>
    {
        public CategoryURIUserControl()
        {
            this.InitializeComponent();
            this.OneWayBind(ViewModel, x => x.MeetingList, y => y.MeetingList.ItemsSource);
        }

        /// <summary>
        /// Track the view models
        /// </summary>
        public CategoryURIViewModel ViewModel
        {
            get { return (CategoryURIViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CategoryURIViewModel), typeof(CategoryURIUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CategoryURIViewModel)value; }
        }
    }
}
