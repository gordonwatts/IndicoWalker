using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Helps coordinate and control the downloading and updating of file (IFile) data
    /// for the app. Shared amongst various VM's as necessary.
    /// </summary>
    /// <remarks>
    /// There should only be one of these at any time for a particular file.
    /// </remarks>
    public class FileDownloadController : ReactiveObject
    {
        /// <summary>
        /// This will trigger a download or an update of the file from the web.
        /// </summary>
        /// <remarks>
        /// All errors (e.g. offline, etc.) are caught.
        /// If the file is cached, then we will look to see if there is an update.
        /// IsDownloading will be updated as appropriate. So will IsDownloaded.
        /// </remarks>
        public ReactiveCommand<object> DownloadOrUpdate { get; private set; }

        /// <summary>
        /// True if data is being downloaded as we speak
        /// </summary>
        public bool IsDownloading
        {
            get { return _isDownloading.Value; }
        }
        private ObservableAsPropertyHelper<bool> _isDownloading;

        /// <summary>
        /// True if there is data for this file available in the local cache
        /// </summary>
        /// <remarks>
        /// This would be true even if there is an update available on the server, or
        /// even if there was an update download in progress
        /// </remarks>
        public bool IsDownloaded
        {
            get { return _isDownloaded.Value; }
        }
        private ObservableAsPropertyHelper<bool> _isDownloaded;

        /// <summary>
        /// Fires each time a new version of the file is available in the cache.
        /// It will fire when there is an update. This fires only when a file
        /// has been downloaded and is available in the cache.
        /// </summary>
        public IObservable<Unit> FileDownloadedAndCached { get; private set; }

        /// <summary>
        /// The file we are looking at
        /// </summary>
        public IFile File { get; private set; }

        /// <summary>
        /// The cache we use - so others can reference it as this is where our files will be stored.
        /// </summary>
        public IBlobCache Cache { get; private set; }

        /// <summary>
        /// Track the global download count across the app.
        /// </summary>
        private static LINQHelpers.LimitGlobalCounter _gLimit = new LINQHelpers.LimitGlobalCounter(4);

        /// <summary>
        /// Reset for testing.
        /// </summary>
        /// <remarks>
        /// TODO: you could argue that we need this only because some tests seem to leave the limit counter non-zero.
        /// This really should not be possible, no matter what error occurs. This should be tested/fixed in the code!
        /// </remarks>
        public static void Reset()
        {
            _gLimit = new LINQHelpers.LimitGlobalCounter(1);
        }

        /// <summary>
        /// Create the download controller for this file
        /// </summary>
        /// <param name="file"></param>
        public FileDownloadController(IFile file, IBlobCache cache = null)
        {
            File = file;
            Cache = cache ?? Blobs.LocalStorage;

            // Download or update the file.
            DownloadOrUpdate = ReactiveCommand.Create();
            var hasCachedValue = DownloadOrUpdate
                .SelectMany(_ => File.GetCacheCreateTime(Cache))
                .Select(dt => dt.HasValue)
                .Publish().RefCount();

            var cacheUpdateRequired = hasCachedValue
                .Where(isCached => isCached)
                .SelectMany(_ => File.CheckForUpdate(Cache))
                .Where(isNewOneEB => isNewOneEB)
                .Select(_ => default(Unit))
                .Publish();
            cacheUpdateRequired.Connect();

            var firstDownloadRequired = hasCachedValue
                .Where(isCached => !isCached)
                .Select(_ => default(Unit));

            var downloadInProgress = new Subject<bool>();

            var downloadRequired =
                Observable.Merge(cacheUpdateRequired, firstDownloadRequired)
                .Do(_ => downloadInProgress.OnNext(true));

            var downloadSuccessful =
                downloadRequired
                .LimitGlobally(goSeq => goSeq
                    .WriteLine("Starting download...")
                    .SelectMany(_ => Download())
                    .SelectMany(data => File.SaveFileInCache(data.Item1, data.Item2, Cache))
                    .WriteLine("  Done download and cache insert"), _gLimit)
                .Finally(() => downloadInProgress.OnNext(false))
                .Do(_ => downloadInProgress.OnNext(false))
                .Catch(Observable.Empty<Unit>())
                .Select(_ => true)
                .Replay(1);
            downloadSuccessful.Connect();

            FileDownloadedAndCached = downloadSuccessful.Select(_ => default(Unit));

            // When we are downloading, set the IsDownloading to true.
            downloadInProgress
                .ToProperty(this, x => x.IsDownloading, out _isDownloading, false);

            // Track the status of the download
            // Note the concatenate when we combine - we very much want this to run
            // in order, no matter what latencies get caught up in the system.
            // This must be run when we are subscribed to, hence the defer.
            var initiallyCached = Observable.Defer(() => File.GetCacheCreateTime(Cache)
                .Delay(TimeSpan.FromMilliseconds(50))
                .Select(dt => dt.HasValue));

            Observable.Concat(initiallyCached, downloadSuccessful)
                .WriteLine(v => string.Format("We are doing the IsDownloaded to {0}", v))
                .ToProperty(this, x => x.IsDownloaded, out _isDownloaded, false);
        }

        /// <summary>
        /// Fetch the data for the file. Return the data as a byte array and also the
        /// date of last update (as seen in the header).
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// TODO: This could currently make two requests to the URL - one to get the data and the other to
        /// get the file header date. We should be able to combine this as one.
        /// </remarks>
        private async Task<Tuple<string, byte[]>> Download()
        {
            // Get the file stream, and write it out.
            var ms = new MemoryStream();
            var r = await File.GetFileStream();
            using (var dataStream = r.Item2)
            {
                await dataStream.BaseStream.CopyToAsync(ms);
            }

            // Get the date from the header that we will need to stash
            var timeStamp = r.Item1;

            // This is what needs to be cached.
            var ar = ms.ToArray();
            return Tuple.Create(timeStamp, ar);
        }
    }
}
