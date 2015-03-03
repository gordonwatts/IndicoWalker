using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
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
        private CompositeDisposable _removeMe = new CompositeDisposable();

        public StartPage()
        {
            this.InitializeComponent();

            this.BindCommand(ViewModel, x => x.SwitchPages, x => x.FindIndicoUrl);
            this.Bind(ViewModel, x => x.MeetingAddress, y => y.IndicoUrl.Text);

            this.Bind(ViewModel, x => x.RecentMeetings, y => y.MainHubView.Sections[1].DataContext);
            this.Loaded += StartPage_Loaded;

            // Do the navagation when we need it here.
            _removeMe.Add(Observable.FromEventPattern<RoutedEventArgs>(GoToSettingsPage, "Click")
                .Subscribe(a => ViewModel.HostScreen.Router.Navigate.Execute(new BasicSettingsViewModel(ViewModel.HostScreen))));
            _removeMe.Add(Observable.FromEventPattern(this, "Unloaded")
                .Subscribe(a => _removeMe.Dispose()));
        }

        void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadRecentMeetings
                .Execute(null);
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
    }
}
