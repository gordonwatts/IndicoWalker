using IWalker.Util;
using IWalker.ViewModels;
using ReactiveUI;
using System;
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
        public BasicSettingsView()
        {
            this.InitializeComponent();

            // Feedback for the user.
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.Error, x => x.ErrorMessage.Text));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.Status, x => x.StatusMessage.Text));
                disposeOfMe(this.Bind(ViewModel, x => x.AutoDownload, x => x.AutoDownload.IsOn));

                // The Indico API key part of the model
                disposeOfMe(this.OneWayBind(ViewModel, x => x.IndicoApiKey, y => y.AddUpdateUserControl.ViewModel));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.ApiKeysForIndico, y => y.ApiKeyList.ItemsSource));
                disposeOfMe(this.WhenAny(x => x.ApiKeyList.SelectedItem, x => x.Value)
                    .Where(x => ViewModel != null)
                    .Subscribe(x => ViewModel.ShowIndicoApiKey.Execute(x)));

                // When they click find, we have to locate a file and go from there.
                disposeOfMe(Observable.FromEventPattern(FindCert, "Click")
                    .Select(a => new FileOpenPicker().ForCert())
                    .Subscribe(op => op.PickSingleFileAndContinue())
                    );

                // WHen the click on delete cache clear everything out.

                disposeOfMe(Observable.FromEventPattern(ClearCache, "Click")
                    .Subscribe(a =>
                    {
                        Blobs.LocalStorage.InvalidateAll();
                    }));

                // This is Windows phone, so after the above we will have to wait until we
                // resume the app.
                disposeOfMe(IWalker.Util.AutoSuspendHelper.IsActivated
                    .Where(args => args.Kind == ActivationKind.PickFileContinuation)
                    .Cast<FileOpenPickerContinuationEventArgs>()
                    .Select(f => f.Files)
                    .Subscribe(files => ViewModel.LoadFiles.Execute(Tuple.Create(files, Password.Password))));

                // Setup the expiration options. We use this clumsy method b.c. we want
                // to avoid ReactiveUI's auto view lookup - we are just going to use ToString for this...
                disposeOfMe(this.WhenAny(x => x.ViewModel, x => x.Value)
                    .Subscribe(x =>
                    {
                        ClearCacheAgenda.ItemsSource = x == null ? null : x.CacheDecayOptions;
                        ClearCacheTalkFiles.ItemsSource = x == null ? null : x.CacheDecayOptions;
                    }));
                disposeOfMe(this.Bind(ViewModel, x => x.CacheDecayAgendas, x => x.ClearCacheAgenda.SelectedItem));
                disposeOfMe(this.Bind(ViewModel, x => x.CacheDecayFiles, x => x.ClearCacheTalkFiles.SelectedItem));
            });


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
