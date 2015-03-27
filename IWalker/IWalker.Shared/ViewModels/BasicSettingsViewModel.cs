using IWalker.Util;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using IWalker.DataModel.Inidco;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Basic settings class.
    /// </summary>
    public class BasicSettingsViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Fired when the page hooks up - so we can do an init of our status.
        /// </summary>
        public ReactiveCommand<Certificate> LookupCertStatus { get; private set; }

        /// <summary>
        /// The list of API keys for indico
        /// </summary>
        public ReactiveList<IndicoApiKey> ApiKeysForIndico { get; private set; }

        /// <summary>
        /// The list of options we might like (or not).
        /// </summary>
        public List<ExpirationOptions.CacheTime> CacheDecayOptions { get; private set; }

        /// <summary>
        /// Get the vm for the indico api key view.
        /// </summary>
        public AddOrUpdateIndicoApiKeyViewModel IndicoApiKey
        {
            get { return _indicoApiKeyVM.Value; }
        }
        private ObservableAsPropertyHelper<AddOrUpdateIndicoApiKeyViewModel> _indicoApiKeyVM;

        /// <summary>
        /// Get/Set the file(talk) cache time.
        /// </summary>
        public ExpirationOptions.CacheTime CacheDecayFiles
        {
            get
            {
                return CacheDecayOptions.Where(x => x.Time == Settings.CacheFilesTime).First();
            }
            set
            {
                if (value.Time != Settings.CacheFilesTime)
                {
                    Settings.CacheFilesTime = value.Time;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get/Set the agenda cache time
        /// </summary>
        public ExpirationOptions.CacheTime CacheDecayAgendas
        {
            get
            {
                return CacheDecayOptions.Where(x => x.Time == Settings.CacheAgendaTime).First();
            }
            set
            {
                if (value.Time != Settings.CacheAgendaTime)
                {
                    Settings.CacheAgendaTime = value.Time;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Basic settings are configured on the view that is attached to this VM.
        /// </summary>
        /// <param name="screen"></param>
        public BasicSettingsViewModel(IScreen screen)
        {
            HostScreen = screen;

            // Look for a currently loaded cert and update the status...
            // We can't start this b.c. the ToProperty is lazy - and it won't
            // fire until Status is data-bound!
            LookupCertStatus = ReactiveCommand.CreateAsyncTask(a => SecurityUtils.FindCert(SecurityUtils.CERNCertName));
            LookupCertStatus
                .Select(c => c == null ? "No Cert Loaded" : string.Format("Loaded (expires {0})", c.ValidTo.ToLocalTime().ToString("yyyy-MM-dd HH:mm")))
                .ToProperty(this, x => x.Status, out _status, "", RxApp.MainThreadScheduler);

            LookupCertStatus
                .ExecuteAsync()
                .Subscribe();

            // Error and status messages...
            var errors = new Subject<string>();
            errors
                .ToProperty(this, x => x.Error, out _error, "", RxApp.MainThreadScheduler);

            // Given a file and a password, see if we can install it as a cert
            // in our internal repository.
            LoadFiles = ReactiveCommand.Create();
            LoadFiles
                .Subscribe(x => errors.OnNext(""));

            var files = LoadFiles
                .Cast<Tuple<IReadOnlyList<StorageFile>, string>>();

            files
                .Where(finfo => finfo.Item1 == null || finfo.Item1.Count != 1)
                .Select(f => "Invalid certificate file")
                .Subscribe(errors);

            files
                .Where(finfo => finfo.Item1 != null && finfo.Item1.Count == 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(mf =>
                {
                    // We use this double subscribe because the readBufferAsync and ImportPfxDataAsync often return exceptions.
                    // If we let the exception bubble all the way up, it terminates the sequence. Which means if the user entered
                    // the wrong password they wouldn't get a chance to try again!
                    return Observable.Return(mf)
                        .SelectMany(async f =>
                        {
                            // Work around for the TplEventListener not working correctly.
                            // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/3e505e04-7f30-4313-aa47-275eaef333dd/systemargumentexception-use-of-undefined-keyword-value-1-for-event-taskscheduled-in-async?forum=wpdevelop
                            await Task.Delay(1);

                            var fs = f.Item1[0] as StorageFile;
                            var buffer = await FileIO.ReadBufferAsync(fs);
                            var cert = CryptographicBuffer.EncodeToBase64String(buffer);

                            await CertificateEnrollmentManager.ImportPfxDataAsync(cert, f.Item2, ExportOption.NotExportable, KeyProtectionLevel.NoConsent, InstallOptions.DeleteExpired, SecurityUtils.CERNCertName);
                            return Unit.Default;
                        });
                })
                .Subscribe(c => c.Subscribe(
                    g => LookupCertStatus.ExecuteAsync().Subscribe(),
                    e => errors.OnNext(e.Message.TakeFirstLine())
                    ));

            // Set/Get the file expiration policy.
            CacheDecayOptions = ExpirationOptions.GetListExpirationOptions();

            // Get the list of indico api keys we are watching
            // and hook up the MV for doing the api key manipulation
            ApiKeysForIndico = new ReactiveList<IndicoApiKey>();
            ApiKeysForIndico.AddRange(IndicoApiKeyAccess.LoadAllKeys());

            Observable.Empty<AddOrUpdateIndicoApiKeyViewModel>()
                .ToProperty(this, x => x.IndicoApiKey, out _indicoApiKeyVM, new AddOrUpdateIndicoApiKeyViewModel(null));

        }
        
        /// <summary>
        /// Fired from the View when we are ready to load a file. It should contain a tuple
        /// of a list of files (with a single entry) and the password (as a string).
        /// </summary>
        public ReactiveCommand<object> LoadFiles { get; private set; }

        /// <summary>
        /// Get the status of the current fetch
        /// </summary>
        public string Status { get { return _status.Value; } }
        private ObservableAsPropertyHelper<string> _status;

        /// <summary>
        /// Any error messages that occured
        /// </summary>
        public string Error { get { return _error.Value; } }
        private ObservableAsPropertyHelper<string> _error;

        /// <summary>
        /// True if we are going to auto-download (or not)
        /// </summary>
        public bool AutoDownload
        {
            get { return Settings.AutoDownloadNewMeeting; }
            set
            {
                if (value != Settings.AutoDownloadNewMeeting)
                {
                    Settings.AutoDownloadNewMeeting = value;
                    this.RaisePropertyChanged();
                }

                }
        }

        /// <summary>
        /// Track the screen we've switch over to.
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// Where we are.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/BasicSettings.xaml"; }
        }
    }
}
