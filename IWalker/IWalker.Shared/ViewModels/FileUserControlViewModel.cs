using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using System.Diagnostics;

namespace IWalker.ViewModels
{
    /// <summary>
    /// The ViewModel for a talk file. We are responsible for downloading, opening, etc. the file.
    /// </summary>
    public class FileUserControlViewModel : ReactiveObject
    {
        /// <summary>
        /// Returns true if the file is cached locally
        /// </summary>
        public bool FileNotCached { get { return _fileNotCached.Value; } }
        private ObservableAsPropertyHelper<bool> _fileNotCached;

        /// <summary>
        /// Get the state of a file download. If true, then the download is in progress.
        /// </summary>
        public bool IsDownloading { get { return _isDownloading.Value; } }
        private ObservableAsPropertyHelper<bool> _isDownloading;

        /// <summary>
        /// The string that connotes the filetype (the extension). Since this
        /// never changes, no need to do anything sophisticated here.
        /// </summary>
        public string DocumentTypeString { get; private set; }

        /// <summary>
        /// Command to fire when the user "clicks" on us.
        /// </summary>
        /// <remarks>
        /// If file isn't downloaded, then download the file.
        /// If file is downloaded, then open the file in another program.
        /// </remarks>
        public ReactiveCommand<object> ClickedUs { get; private set; }

        /// <summary>
        /// Fires each time we download/update a file (in the cache).
        /// Will not fire if the file is already in the cache.
        /// </summary>
        public IObservable<Unit> DownloadedFile { get; private set; }

        /// <summary>
        /// The controller that takes care of actual file downloading, etc.
        /// </summary>
        private FileDownloadController _fileDownloader;

        /// <summary>
        /// Initialize all of our behaviors.
        /// </summary>
        /// <param name="file"></param>
        public FileUserControlViewModel(IFile file)
        {
            // Save the document type for the UI
            DocumentTypeString = file.FileType.ToUpper();

            // Setup the actual file downloader
            _fileDownloader = new FileDownloadController(file);

            // Now, hook up our UI indicators to the download control.

            _fileDownloader.WhenAny(x => x.IsDownloading, x => x.Value)
                .ToProperty(this, x => x.IsDownloading, out _isDownloading, false, RxApp.MainThreadScheduler);

            _fileDownloader.WhenAny(x => x.IsDownloaded, x => x.Value)
                .Select(x => !x)
                .ToProperty(this, x => x.FileNotCached, out _fileNotCached, true, RxApp.MainThreadScheduler);

            DownloadedFile = _fileDownloader.FileDownloadedAndCached;

            // Allow them to download a file.
            var canDoDownload = _fileDownloader.WhenAny(x => x.IsDownloading, x => x.Value)
                .Select(x => !x);
            ClickedUs = ReactiveCommand.Create(canDoDownload);

            ClickedUs
                .Where(_ => !_fileDownloader.IsDownloaded)
                .InvokeCommand(_fileDownloader.DownloadOrUpdate);

            // Opening the file is a bit more complex. It happens only when the user clicks the button a second time.
            // Requires us to write a file to the local cache.
            ClickedUs
                .Where(_ => _fileDownloader.IsDownloaded)
                .SelectMany(_ => file.GetFileFromCache(Blobs.LocalStorage))
                .SelectMany(async stream =>
                {
                    var fname = string.Format("{0}.{1}", file.DisplayName.CleanFilename(), file.FileType).CleanFilename();
                    var fdate = await file.GetCacheCreateTime();
                    var folder = fdate.HasValue ? fdate.Value.ToString().CleanFilename() : "Unknown Cache Time";

                    // Write the file. If it is already written, then we will just return it (e.g. assume it is the same).
                    // 0x800700B7 (-2147024713) is the error code for file already exists.
                    return CreateFile(folder, fname)
                        .SelectMany(f => f.OpenStreamForWriteAsync())
                        .SelectMany(async fstream =>
                        {
                            try
                            {
                                using (var readerStream = stream.AsStreamForRead())
                                {
                                    await readerStream.CopyToAsync(fstream);
                                }
                            }
                            finally
                            {
                                fstream.Dispose();
                            }
                            return default(Unit);
                        })
                        .Catch<Unit, Exception>(e =>
                        {
                            if (e.HResult == unchecked((int)0x800700B7))
                                return Observable.Return(default(Unit));
                            return Observable.Throw<Unit>(e);
                        })
                        .SelectMany(_ => GetExistingFile(folder, fname));
                })
                .SelectMany(f => f)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(f =>
                {
                    return Observable.FromAsync(async _ => await Launcher.LaunchFileAsync(f))
                        .Select(good => Tuple.Create(f, good))
                        .Catch(Observable.Return(Tuple.Create(f, false)));
                })
                .Where(g => g.Item2 == false)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(f => 
                    {
                        return Observable.FromAsync(async _ => await Launcher.LaunchFileAsync(f.Item1, new LauncherOptions() { DisplayApplicationPicker = true }))
                            .Catch(Observable.Return(false));
                    })
                .Where(g => g == false)
                .Subscribe(
                    g => { throw new InvalidOperationException(string.Format("Unable to open file {0}.", file.DisplayName)); },
                    e => { throw new InvalidOperationException(string.Format("Unable to open file {0}.", file.DisplayName), e); }
                );

            // Init the UI from the cache. We want to do one or the other
            // because the download will fetch from the cache first. So no need to
            // fire them both off.
            if (Settings.AutoDownloadNewMeeting)
            {
                _fileDownloader.DownloadOrUpdate.Execute(null);
            }
        }

        /// <summary>
        /// Create a file in a special folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fname"></param>
        /// <returns></returns>
        private IObservable<StorageFile> CreateFile(string folder, string fname)
        {
            var sFolder = Observable.FromAsync(_ => ApplicationData.Current.TemporaryFolder.GetFolderAsync(folder).AsTask())
                .Catch(Observable.FromAsync(_ => ApplicationData.Current.TemporaryFolder.CreateFolderAsync(folder).AsTask()));

            var sFile = sFolder
                .SelectMany(sf => sf.CreateFileAsync(fname, CreationCollisionOption.FailIfExists));

            return sFile;
        }

        /// <summary>
        /// Return a file that already exists (or bad things happen).
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fname"></param>
        /// <returns></returns>
        private IObservable<StorageFile> GetExistingFile(string folder, string fname)
        {
            return Observable.FromAsync(_ => ApplicationData.Current.TemporaryFolder.GetFolderAsync(folder).AsTask())
                .SelectMany(sf => sf.GetFileAsync(fname));
        }
    }
}
