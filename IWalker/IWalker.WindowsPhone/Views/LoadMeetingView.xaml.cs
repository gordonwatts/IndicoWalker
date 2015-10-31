using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace IWalker.Views
{
    /// <summary>
    /// Allows the user to open a meeting from a URL that they paste into the
    /// UI. We delegate everything, actually.
    /// </summary>
    public sealed partial class LoadMeetingView : Page, IViewFor<OpenURLControlViewModel>
    {
        public LoadMeetingView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.BindCommand(ViewModel, x => x.SwitchPages, x => x.FindIndicoUrl));
                disposeOfMe(this.Bind(ViewModel, x => x.MeetingAddress, y => y.IndicoUrl.Text));
            });
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public OpenURLControlViewModel ViewModel
        {
            get { return (OpenURLControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(OpenURLControlViewModel), typeof(LoadMeetingView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (OpenURLControlViewModel)value; }
        }
    }
}
