using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive;
using System;
using System.Reactive.Linq;
using System.Text;
using Windows.Storage;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
using System.Threading.Tasks;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Basic settings class.
    /// </summary>
    public class BasicSettingsViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Basic settings are configured on the view that is attached to this VM.
        /// </summary>
        /// <param name="screen"></param>
        public BasicSettingsViewModel(IScreen screen)
        {
            HostScreen = screen;

            // Error and status messages...
            var errors = new Subject<string>();
            errors
                .ToProperty(this, x => x.Error, out _error, "", RxApp.MainThreadScheduler);

            var status = new Subject<string>();
            status
                .ToProperty(this, x => x.Status, out _status, "", RxApp.MainThreadScheduler);

            LoadFiles = ReactiveCommand.Create();
            var files = LoadFiles
                .Cast<Tuple<IReadOnlyList<StorageFile>, string>>();

            files
                .Where(finfo => finfo.Item1 == null || finfo.Item1.Count != 1)
                .Select(f => "Invalid certificate file")
                .Subscribe(errors);

            files
                .Where(finfo => finfo.Item1 != null && finfo.Item1.Count == 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(async f =>
                {
                    await Task.Delay(1);

                    var fs = f.Item1[0] as StorageFile;
                    var buffer = await FileIO.ReadBufferAsync(fs);
                    var cert = CryptographicBuffer.EncodeToBase64String(buffer);

                    await CertificateEnrollmentManager.ImportPfxDataAsync(cert, f.Item2, ExportOption.NotExportable, KeyProtectionLevel.NoConsent, InstallOptions.DeleteExpired, "CERNCert");
                    return Unit.Default;
                })
                .Subscribe(
                    c => status.OnNext("New Cert Loaded"),
                    e => errors.OnNext(string.Format("Failed: {0}", e.Message))
                );
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
