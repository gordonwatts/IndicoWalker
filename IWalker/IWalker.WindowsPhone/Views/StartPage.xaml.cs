using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
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

                // Do the navagation when we need it here.
                disposeOfMe(Observable.FromEventPattern<RoutedEventArgs>(GoToSettingsPage, "Click")
                    .Subscribe(a => ViewModel.HostScreen.Router.Navigate.Execute(new BasicSettingsViewModel(ViewModel.HostScreen))));
                disposeOfMe(Observable.FromEventPattern<RoutedEventArgs>(GoToLoadPage, "Click")
                    .Subscribe(a => ViewModel.HostScreen.Router.Navigate.Execute(new OpenURLControlViewModel(ViewModel.HostScreen))));

                // Update everything
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
        /// When the user clicks on an existing meeting, this is where we end up.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.OpenMRUMeeting.Execute(e.ClickedItem);
        }

        /// <summary>
        /// Fired when an upcoming meeting is selected.
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
