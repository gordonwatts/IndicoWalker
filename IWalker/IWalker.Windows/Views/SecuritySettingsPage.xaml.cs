using IWalker.Util;
using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IWalker.Views
{
    /// <summary>
    /// Settings for all things security
    /// </summary>
    public sealed partial class SecuritySettingsPage : Page, IViewFor<BasicSettingsViewModel>
    {
        /// <summary>
        /// Track the eventual dispose we will have to do of the hanging
        /// subscriptions.
        /// </summary>
        private CompositeDisposable _ridOfMe = new CompositeDisposable();

        public SecuritySettingsPage()
        {
            this.InitializeComponent();

            // Feedback for the user.
            this.OneWayBind(ViewModel, x => x.Error, x => x.ErrorMessage.Text);
            this.OneWayBind(ViewModel, x => x.Status, x => x.StatusMessage.Text);
            this.BindCommand(ViewModel, x => x.HostScreen.Router.NavigateBack, y => y.backButton);

            // When they click find, we have to locate a file and go from there.
            var basicFindFile = Observable.FromEventPattern(FindCert, "Click")
                .Select(a => new FileOpenPicker().ForCert())
                .SelectMany(op => op.PickSingleFileAsync());

            // This is the store, so as soon as we have that stuff, we can cycle straight into doing this.
            _ridOfMe.Add(
                basicFindFile
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(files => ViewModel.LoadFiles.Execute(Tuple.Create(new StorageFile[] { files } as IReadOnlyList<StorageFile>, Password.Password)))
            );

            // Make sure to get rid of any connections we had to make ad-hoc.
            _ridOfMe.Add(Observable.FromEventPattern(this, "Unloaded")
                .Subscribe(a => _ridOfMe.Dispose()));
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public BasicSettingsViewModel ViewModel
        {
            get { return (BasicSettingsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(BasicSettingsViewModel), typeof(SecuritySettingsPage), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (BasicSettingsViewModel)value; }
        }
    }
}
