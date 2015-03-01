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
        public bool HaveFileCached { get { return _haveFileCached.Value; } }
        private ObservableAsPropertyHelper<bool> _haveFileCached;

        /// <summary>
        /// Command to fire when the user "clicks" on us.
        /// </summary>
        /// <remarks>
        /// If file isn't downloaded, then download the file.
        /// If file is downloaded, then open the file in another program.
        /// </remarks>
        public ReactiveCommand<object> ClickedUs { get; private set; }

        /// <summary>
        /// Initialize all of our behaviors.
        /// </summary>
        /// <param name="file"></param>
        public FileUserControlViewModel(IFile file)
        {
            _file = file;

            // Extract from cache or download it.
            var cmdLookAtCache = ReactiveCommand.CreateAsyncObservable(token => _file.GetFileFromCache());
            var cmdDownloadNow = ReactiveCommand.CreateAsyncObservable(_ => _file.GetAndUpdateFileOnce());

            var initiallyCached = cmdLookAtCache.Merge(cmdDownloadNow)
                .Select(f => f != null)
                .ToProperty(this, x => x.HaveFileCached, out _haveFileCached, false);

            // Lets see if they want to download the file.
            ClickedUs = ReactiveCommand.Create();

            ClickedUs
                .Where(_ => _haveFileCached.Value == false)
                .Subscribe(_ => cmdDownloadNow.Execute(null));

            // Opening the file is a bit more complex
            ClickedUs
                .Where(_ => _haveFileCached.Value == true)
                .SelectMany(_ => _file.GetFileFromCache())
                .SelectMany(async stream =>
                {
                    var fname = string.Format("{1}-{0}.{2}", await _file.GetCacheCreateTime(), _file.DisplayName, _file.FileType).CleanFilename();

                    // Write the file. If it is already written, then we will just return it (e.g. assume it is the same).
                    // 0x800700B7 (-2147024713) is the error code for file already exists.
                    return Observable.FromAsync(token => ApplicationData.Current.TemporaryFolder.CreateFileAsync(fname, CreationCollisionOption.FailIfExists).AsTask())
                        .SelectMany(f => f.OpenStreamForWriteAsync())
                        .SelectMany(async fstream => { await stream.AsStreamForRead().CopyToAsync(fstream); return default(Unit); })
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
                .SelectMany(async f =>
                {
                    var v = await Launcher.LaunchFileAsync(f);
                    return Tuple.Create(f, v);
                })
                .Where(g => g.Item2 == false)
                .SelectMany(f => Launcher.LaunchFileAsync(f.Item1, new LauncherOptions() { DisplayApplicationPicker = true }))
                .Where(g => g == false)
                .Subscribe(
                    g => { throw new InvalidOperationException(string.Format("Unable to open file {0}.", _file.DisplayName)); },
                    e => { throw new InvalidOperationException(string.Format("Unable to open file {0}.", _file.DisplayName), e); }
                );

            // Init the UI from the cache
            cmdLookAtCache.ExecuteAsync().Subscribe();
        }
    }
}
