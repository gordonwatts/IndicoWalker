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
        /// The file we represent.
        /// </summary>
        private IFile _file;

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
        /// Initialize all of our behaviors.
        /// </summary>
        /// <param name="file"></param>
        public FileUserControlViewModel(IFile file)
        {
            _file = file;

            // Save the document type for the UI
            DocumentTypeString = _file.FileType.ToUpper();

            // Extract from cache or download it.
            // -- GetFileFromCache will not send anything along if there is nothing in the cache, so expect that not to fire at all.
            var cmdLookAtCache = ReactiveCommand.CreateAsyncObservable(token => _file.GetFileFromCache());
            ReactiveCommand<IRandomAccessStream> cmdDownloadNow = null;
            if (_file.IsValid)
            {
                cmdDownloadNow = ReactiveCommand.CreateAsyncObservable(_ => _file.UpdateFileOnce());
            }
            else
            {
                cmdDownloadNow = ReactiveCommand.CreateAsyncObservable(_ => Observable.Empty<IRandomAccessStream>());
            }

            // UI Updating
            // - When we have downloaded already, turn off the little download button.
            // - During download, run the ring progress bar thing.
            // We access ".Value" to force the side-effect of causing a subscription. We have to do this, as ToProperty won't
            // untill the Bind occurs, and the command that we execute below will probably happen before we get a chance.
            cmdLookAtCache.Concat(cmdDownloadNow)
                .Select(f => f == null)
                .WriteLine("Got file {0}.", file.UniqueKey)
                .Merge<bool>(cmdDownloadNow.IsExecuting.Where(x => x==true).Select(_ => false))
                .ToProperty(this, x => x.FileNotCached, out _fileNotCached, true);
            var bogus = _fileNotCached.Value;

            var seenFirstFile = cmdDownloadNow
                .Where(f => f != null)
                .Select(_ => true);

            cmdDownloadNow.IsExecuting
                .CombineLatest(seenFirstFile.StartWith(false), (isExe, seenFF) => isExe && !seenFF)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(x => Debug.WriteLine("We are executing: {0}", x))
                .ToProperty(this, x => x.IsDownloading, out _isDownloading, false);
            bogus = _isDownloading.Value;

            // Notify anyone in the world that is going to care. We can only
            // fire when there has been a real-update to the file.

            DownloadedFile = cmdDownloadNow
                .Select(_ => default(Unit));


            // Lets see if they want to download the file.
            var canDoDownload = cmdDownloadNow.IsExecuting.Select(x => !x);
            ClickedUs = ReactiveCommand.Create(canDoDownload);

            ClickedUs
                .Where(_ => _fileNotCached.Value == true)
                .Subscribe(_ => cmdDownloadNow.Execute(null));

            // Opening the file is a bit more complex. It happens only when the user clicks the button a second time.
            // Requires us to write a file to the local cache.
            ClickedUs
                .Where(_ => _fileNotCached.Value == false)
                .SelectMany(_ => _file.GetFileFromCache())
                .SelectMany(async stream =>
                {
                    var fname = string.Format("{1}-{0}.{2}", await _file.GetCacheCreateTime(), _file.DisplayName, _file.FileType).CleanFilename();

                    // Write the file. If it is already written, then we will just return it (e.g. assume it is the same).
                    // 0x800700B7 (-2147024713) is the error code for file already exists.
                    return Observable.FromAsync(token => ApplicationData.Current.TemporaryFolder.CreateFileAsync(fname, CreationCollisionOption.FailIfExists).AsTask())
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
                        .SelectMany(_ => ApplicationData.Current.TemporaryFolder.GetFileAsync(fname));
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
                    g => { throw new InvalidOperationException(string.Format("Unable to open file {0}.", _file.DisplayName)); },
                    e => { throw new InvalidOperationException(string.Format("Unable to open file {0}.", _file.DisplayName), e); }
                );

            // Init the UI from the cache. We want to do one or the other
            // because the download will fetch from the cache first. So no need to
            // fire them both off.
            cmdLookAtCache.ExecuteAsync().Subscribe();
            if (Settings.AutoDownloadNewMeeting)
            {
                cmdDownloadNow.ExecuteAsync().Subscribe();
            }
        }
    }
}
