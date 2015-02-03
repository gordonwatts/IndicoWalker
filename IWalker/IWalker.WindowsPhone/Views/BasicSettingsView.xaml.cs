using IWalker.Util;
using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BasicSettingsView : Page, IViewFor<BasicSettingsViewModel>
    {
        /// <summary>
        /// Track subscriptions we want to kill off.
        /// </summary>
        private CompositeDisposable _ridOfMe = new CompositeDisposable();

        public BasicSettingsView()
        {
            this.InitializeComponent();

            // Feedback for the user.
            this.OneWayBind(ViewModel, x => x.Error, x => x.ErrorMessage.Text);
            this.OneWayBind(ViewModel, x => x.Status, x => x.StatusMessage.Text);

            // When they click find, we have to locate a file and go from there.
            _ridOfMe.Add(
                Observable.FromEventPattern(FindCert, "Click")
                .Select(a => new FileOpenPicker().ForCert())
                .Subscribe(op => op.PickSingleFileAndContinue())
                );

            // This is Windows phone, so after the above we will have to wait until we
            // resume the app.
            var phoneFileList = IWalker.Util.AutoSuspendHelper.IsActivated
                .Where(args => args.Kind == ActivationKind.PickFileContinuation)
                .Cast<FileOpenPickerContinuationEventArgs>()
                .Select(f => f.Files)
                .Subscribe(files => ViewModel.LoadFiles.Execute(Tuple.Create(files, Password.Password)));

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
            DependencyProperty.Register("ViewModel", typeof(BasicSettingsViewModel), typeof(BasicSettingsView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (BasicSettingsViewModel)value; }
        }
    }
}
