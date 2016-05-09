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
    public sealed partial class StartPage : Page, IViewFor<StartPageViewModel>
    {
        public StartPage()
        {
            this.InitializeComponent();

            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.RecentMeetings, y => y.MainHubView.Sections[1].DataContext));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.UpcomingMeetings, y => y.MainHubView.Sections[0].DataContext));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.OpenURLControlVM, y => y.OpenURLControl.ViewModel));

                ViewModel.UpdateUpcomingMeetings
                    .Execute(null);
            });
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public StartPageViewModel ViewModel
        {
            get { return (StartPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(StartPageViewModel), typeof(StartPage), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (StartPageViewModel)value; }
        }

        /// <summary>
        /// Something in the MRU list has been clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.OpenMRUMeeting.Execute(e.ClickedItem);
        }

        /// <summary>
        /// They have clicked on a meeting, so we should page to it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.OpenUpcomingMeeting.Execute(e.ClickedItem);
        }

        /// <summary>
        /// They want to see the full calendar list now!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFullCalendarListsList(object sender, RoutedEventArgs e)
        {
            ViewModel.HostScreen.Router.Navigate.Execute(new CategoryAllPageViewModel(ViewModel.HostScreen));
        }
    }
}
