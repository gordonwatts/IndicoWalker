﻿using IWalker.ViewModels;
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
    public sealed partial class MeetingPage : Page, IViewFor<MeetingPageViewModel>
    {
        public MeetingPage()
        {
            this.InitializeComponent();

            // Bind everything together we need.
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.MeetingTitle, y => y.MeetingTitle.Text));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.StartTime, y => y.StartTime.Text));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.Sessions, y => y.SessionList.ItemsSource));

                disposeOfMe(this.OneWayBind(ViewModel, x => x.Days, y => y.ConferenceDayPicker.ItemsSource));
                disposeOfMe(this.Bind(ViewModel, x => x.DisplayDayIndex, y => y.ConferenceDayPicker.SelectedIndex));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.Days.Count, y => y.ConferenceDayPicker.Visibility, cnt => cnt <= 1 ? Visibility.Collapsed : Visibility.Visible));

                disposeOfMe(this.BindCommand(ViewModel, x => x.OpenMeetingInBrowser, y => y.OpenInBrowser));

                disposeOfMe(this.OneWayBind(ViewModel, x => x.MeetingIsReadyForDisplay, y => y.LoadingProgress.Visibility, val => val ? Visibility.Collapsed : Visibility.Visible));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.MeetingIsEmpty, y => y.NothingFound.Visibility));

                // Start the data population. Do it here to make sure that everything else has already been setup.
                disposeOfMe(this.WhenAny(x => x.ViewModel, x => x.Value).Where(vm => vm != null).Subscribe(vm => vm.StartMeetingUpdates.Execute(null)));

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
