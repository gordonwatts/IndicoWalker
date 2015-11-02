using IWalker.Util;
using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;
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
    public sealed partial class MeetingPage : Page, IViewFor<MeetingPageViewModel>
    {
        public MeetingPage()
        {
            Debug.WriteLine("Stargin up a XAML meeting page.");
            this.InitializeComponent();

            // Bind everything together we need.
            backButton.WireAsBackButton();

            // Collect everything we are going to want to dispose of.
            var gc = new CompositeDisposable();

            gc.Add(this.OneWayBind(ViewModel, x => x.MeetingTitle, y => y.MeetingTitle.Text));
            gc.Add(this.OneWayBind(ViewModel, x => x.StartTime, y => y.StartTime.Text));
            gc.Add(this.OneWayBind(ViewModel, x => x.Sessions, y => y.SessionList.ItemsSource));
            gc.Add(this.OneWayBind(ViewModel, x => x.Days, y => y.ConferenceDayPicker.ItemsSource));
            gc.Add(this.Bind(ViewModel, x => x.DisplayDayIndex, y => y.ConferenceDayPicker.SelectedIndex));
            gc.Add(this.OneWayBind(ViewModel, x => x.Days.Count, y => y.ConferenceDayPicker.Visibility, cnt => cnt <= 1 ? Visibility.Collapsed : Visibility.Visible));
            gc.Add(this.BindCommand(ViewModel, x => x.OpenMeetingInBrowser, y => y.OpenInBrowser));
            gc.Add(this.OneWayBind(ViewModel, x => x.MeetingIsEmpty, y => y.NothingFound.Visibility));
            gc.Add(this.OneWayBind(ViewModel, x => x.MeetingIsReadyForDisplay, y => y.LoadingProgress.Visibility, val => val ? Visibility.Collapsed : Visibility.Visible));

            // Start the data population. Do it here to make sure that everything else has already been setup.
            gc.Add(this.WhenAny(x => x.ViewModel, x => x.Value).Where(vm => vm != null).DistinctUntilChanged().Subscribe(vm => vm.StartMeetingUpdates.Execute(null)));

            // And get rid of it when it is time.
            this.WhenActivated(disposeOfMe =>
            {
                if (gc != null)
                {
                    disposeOfMe(gc);
                    gc = null;
                }
            });
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
