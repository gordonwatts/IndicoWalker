﻿using IWalker.ViewModels;
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

            this.BindCommand(ViewModel, x => x.SwitchPages, x => x.FindIndicoUrl);
            this.Bind(ViewModel, x => x.MeetingAddress, y => y.IndicoUrl.Text);

            // Cert stuff
            this.BindCommand(ViewModel, x => x.LoadCert, x => x.LoadIt);
            this.Bind(ViewModel, x => x.CertPassword, x => x.CertPassword.Text);
            this.Bind(ViewModel, x => x.CertStateText, x => x.CertStatus.Text);
            this.BindCommand(ViewModel, x => x.StartSequence, x => x.Start);
            this.Bind(ViewModel, x => x.CertPasswordEnabled, x => x.CertControls.Visibility);
            // Navigation.
            //this.NavigationCacheMode = NavigationCacheMode.Required;
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
    }
}