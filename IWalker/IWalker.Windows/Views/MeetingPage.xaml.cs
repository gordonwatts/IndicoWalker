using IWalker.Util;
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
    public sealed partial class MeetingPage : Page, IViewFor<MeetingPageViewModel>
    {
        public MeetingPage()
        {
            this.InitializeComponent();

            // Bind everything together we need.
            backButton.WireAsBackButton();
            this.Bind(ViewModel, x => x.MeetingTitle, y => y.MeetingTitle.Text);
            this.Bind(ViewModel, x => x.StartTime, y => y.StartTime.Text);
            this.OneWayBind(ViewModel, x => x.Sessions, y => y.SessionList.ItemsSource);
            this.OneWayBind(ViewModel, x => x.Days, y => y.ConferenceDayPicker.ItemsSource);
            this.Bind(ViewModel, x => x.DisplayDayIndex, y => y.ConferenceDayPicker.SelectedIndex);
            this.OneWayBind(ViewModel, x => x.Days.Count, y => y.ConferenceDayPicker.Visibility, cnt => cnt <= 1 ? Visibility.Collapsed : Visibility.Visible);
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public MeetingPageViewModel ViewModel
        {
            get { return (MeetingPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MeetingPageViewModel), typeof(MeetingPage), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MeetingPageViewModel)value; }
        }
    }
}
