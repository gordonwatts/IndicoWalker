using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    /// <summary>
    /// View for the talks in a session
    /// </summary>
    public sealed partial class SessionUserControl : UserControl, IViewFor<SessionUserControlViewModel>
    {
        public SessionUserControl()
        {
            this.InitializeComponent();
            this.OneWayBind(ViewModel, x => x.Title, y => y.SessionTitle.Text);
            this.OneWayBind(ViewModel, x => x.Talks, y => y.TalkList.ItemsSource);
        }

        /// <summary>
        /// Hold onto the view model, which we will need for doing all sorts of things.
        /// </summary>
        public SessionUserControlViewModel ViewModel
        {
            get { return (SessionUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SessionUserControlViewModel), typeof(SessionUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (SessionUserControlViewModel)value; }
        }
    }
}
