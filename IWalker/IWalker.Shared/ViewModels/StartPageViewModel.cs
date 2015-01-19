using CERNSSO;
using IWalker.DataModel.Inidco;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Main page with a simple button on it.
    /// </summary>
    public class StartPageViewModel :  ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// When clicked, it will cause the page to switch and the text to be saved.
        /// </summary>
        public ReactiveCommand<object> SwitchPages { get; set; }

        /// <summary>
        /// Start file picker to load a cert
        /// </summary>
        public ReactiveCommand<object> LoadCert { get; set; }

        public ReactiveCommand<object> StartSequence { get; set; }

        /// <summary>
        /// The meeting address (bindable).
        /// </summary>
        public string MeetingAddress
        {
            get { return _meetingAddress; }
            set { this.RaiseAndSetIfChanged(ref _meetingAddress, value); }
        }
        private string _meetingAddress;

        /// <summary>
        /// Password we use to decode the cert when we load it.
        /// </summary>
        public string CertPassword
        {
            get { return _password; }
            private set { this.RaiseAndSetIfChanged(ref _password, value); }
        }
        private string _password;

        /// <summary>
        /// True if the cert password and button should show up right now.
        /// </summary>
        public Visibility CertPasswordEnabled
        {
            get { return _passwordEnabled.Value; }
        }
        private readonly ObservableAsPropertyHelper<Visibility> _passwordEnabled;

        /// <summary>
        /// Returns the state of the cert for the UI.
        /// </summary>
        public string CertStateText
        {
            get { return _certStateText.Value; }
        }
        private readonly ObservableAsPropertyHelper<string> _certStateText;

        /// <summary>
        /// What is the current load state
        /// </summary>
        enum CertLoadState
        {
            SearchingInAppContainer,
            NotLoaded,
            LoadingFromFile,
            Loaded
        }

        /// <summary>
        /// Setup the page
        /// </summary>
        public StartPageViewModel(IScreen screen)
        {
            HostScreen = screen;

            // Make sure nothing is null..
            CertPassword = "";

            // We can switch pages only when the user has written something into the meeting address text.
            var canNavagateAway = this.WhenAny(x => x.MeetingAddress, x => !string.IsNullOrWhiteSpace(x.Value));
            SwitchPages = ReactiveCommand.Create(canNavagateAway);

            // When we navigate away, we should save the text and go
            SwitchPages
                .Select(x => MeetingAddress)
                .Subscribe(addr =>
                {
                    Settings.LastViewedMeeting = addr;
                    HostScreen.Router.Navigate.Execute(new MeetingPageViewModel(HostScreen, ConvertToIMeeting(addr)));
                });

            // The state of the cert loading process, and attach it to the UI and setup the state machine.
            var _certState = new Subject<CertLoadState>();

            _certStateText = _certState
                .Select(c => c.ToString())
                .ToProperty(this, x => x.CertStateText, "Initalizing...", RxApp.MainThreadScheduler);

            StartSequence = ReactiveCommand.Create();
            StartSequence.Subscribe(addr =>
                {
                    _certState.OnNext(CertLoadState.SearchingInAppContainer);
                });

            // Do the search in the local app container for a cert that is already in there.
            var lookForCertInLocalApp = _certState
                .Where(s => s == CertLoadState.SearchingInAppContainer)
                .SelectMany(async o => await FindCert("CERNTestCert")); // TODO: how do we "buffer" this result?

            lookForCertInLocalApp
                .Where(c => c != null)
                .Do(c => WebAccess.LoadCertificate(c))
                .Subscribe(c => _certState.OnNext(CertLoadState.Loaded),
                    e => _certState.OnNext(CertLoadState.NotLoaded));
            lookForCertInLocalApp
                .Where(c => c == null)
                .Subscribe(c => _certState.OnNext(CertLoadState.NotLoaded));

            // only let folks look at the password stuff when they can use it.
            _passwordEnabled = _certState
                .Select(c => c == CertLoadState.NotLoaded ? Visibility.Visible : Visibility.Collapsed)
                .ToProperty(this, x => x.CertPasswordEnabled, Visibility.Collapsed, RxApp.MainThreadScheduler);

            // The load cert command, which can only be executed when nothing is loaded and there is a password.
            var canTryToPickCert = this.WhenAny(x => x.CertPassword, cp => !string.IsNullOrWhiteSpace(cp.Value))
                .CombineLatest(_certState, (ok, cs) => ok && cs == CertLoadState.NotLoaded);
            LoadCert = ReactiveCommand.Create(canTryToPickCert);

            var xme = new HtmlAgilityPack.HtmlDocument();

            // Allow the user to load a cert. It is always possible to do the create.
            var setupAsk = LoadCert.Select(o =>
            {
                // Configure for a boring cert picking.
                var op = new FileOpenPicker();
                op.CommitButtonText = "Use Certificate";
                op.SettingsIdentifier = "OpenCert";
                op.FileTypeFilter.Add(".p12");
                op.FileTypeFilter.Add(".pfx");
                op.ViewMode = PickerViewMode.List;
                return op;
            });

#if WINDOWS_PHONE_APP
            setupAsk
                .Subscribe(op => op.PickSingleFileAndContinue());

            int j = 0;
            RxApp.SuspensionHost.IsResuming
                .Subscribe(o => j = 10);
#else
            var storageFile = setupAsk.SelectMany(async fp => await fp.PickSingleFileAsync());
            LoadCertIntoAppContainer(_certState, storageFile);
#endif

            // Clear out the password after we've got something loaded. Make sure it is only enabled when we want it enabled.
            var clearItOut = _certState
                .Where(c => c == CertLoadState.Loaded)
                .Subscribe(c => CertPassword = "");

            // Setup the first value for the last time we ran to make life a little simpler.
            MeetingAddress = Settings.LastViewedMeeting;
        }

        private void LoadCertIntoAppContainer(Subject<CertLoadState> _certState, IObservable<Windows.Storage.StorageFile> storageFile)
        {
            storageFile.SelectMany(async f =>
            {
                var buffer = await Windows.Storage.FileIO.ReadBufferAsync(f);
                var cert = Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(buffer);

                // Try loading it into the local store.
                await CertificateEnrollmentManager.ImportPfxDataAsync(cert, CertPassword, ExportOption.NotExportable, KeyProtectionLevel.NoConsent, InstallOptions.DeleteExpired, "CERNTestCert");
                return f;
            })
            .Subscribe(
                c => _certState.OnNext(CertLoadState.SearchingInAppContainer),
                e => _certState.OnNext(CertLoadState.NotLoaded)
            );
        }

        /// <summary>
        /// Returns a certificate with a given name
        /// </summary>
        /// <param name="certName">Name of cert we are going to look for</param>
        /// <returns>null if the cert isn't there, otherwise the cert that was found.</returns>
        private async Task<Certificate> FindCert(string certName)
        {
            var query = new CertificateQuery();
            query.FriendlyName = certName;
            var certificates = await CertificateStores.FindAllAsync(query);

            if (certificates.Count != 1)
            {
                return null;
            }
            return certificates[0];
        }

        /// <summary>
        /// Convert text entry to some sort of address.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private IMeetingRef ConvertToIMeeting(string addr)
        {
            return new IndicoMeetingRef(addr);
        }

        /// <summary>
        /// Track the home screen.
        /// </summary>
        public IScreen HostScreen {get; private set;}

        /// <summary>
        /// Where we will be located.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/home"; }
        }
    }
}
