using Akavache;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Windows.Data.Pdf;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Represents a VM that is a PDF file.
    /// </summary>
    public class PDFFile : ReactiveObject
    {
        /// <summary>
        /// Get the number of pages this PDF file has
        /// </summary>
        public int NumberOfPages
        {
            get { return _nPages.Value; }
        }
        private ObservableAsPropertyHelper<int> _nPages;

        /// <summary>
        /// Return PdfPage and cache key. The PdfPage will render only once per subscription. THe cache key
        /// can be used to cache images in our local data db.
        /// </summary>
        /// <returns></returns>
        public IObservable<Tuple<string, IObservable<PdfPage>>> GetPageStreamAndCacheInfo(int index)
        {
            var postfixCacheName = string.Format("-p{0}", index);
            return _pdfAndCacheKey
                .Select(info => Tuple.Create(info.Item1 + postfixCacheName, info.Item2.Select(doc => doc.GetPage((uint)index))));
        }

        /// <summary>
        /// Get ourselves setup and going given a file source.
        /// </summary>
        /// <param name="fileSource"></param>
        public PDFFile(FileDownloadController fileSource)
        {
            // Each time a new file shows up, get the file and decode it.
            var isDownloaded = fileSource
                .WhenAny(x => x.IsDownloaded, x => x.Value)
                .Where(dwn => dwn == true)
                .Select(_ => default(Unit));

            var newFile = fileSource
                .FileDownloadedAndCached;

            // Load it up as a real PDF document. Make sure we don't do it more than once.
            // Note the publish below - otherwise we will miss it going by if it happens too
            // fast.
            var cacheKey = Observable.Merge(isDownloaded, newFile)
                .SelectMany(_ => fileSource.File.GetCacheCreateTime(fileSource.Cache))
                .Select(date => string.Format("{0}-{1}", fileSource.File.UniqueKey, date.ToString()))
                .DistinctUntilChanged();

            // This will render a document each time it is called. Note the
            // the Replay at the end. We want to use the same file for everyone. And, each time
            // a new file comes through, the cacheKey should be updated, and that should cause
            // this to be re-subscribed. So this is good ONLY FOR ONE FILE at a time. Re-subscribe to
            // get a new version of the file.
            // -> Check that we don't need a RefCount - if we did, we'd have to be careful that getting the # of pages
            // didn't cause one load, and then the rendering caused another load. The sequence might matter...
            // -> The Take(1) is to make sure we do this only once. Otherwise this sequence could remain open forever,
            //    and that will cause problems with the GetOrFetchObject, which expects to use only the last time in the sequence
            //    it looks at!
            Func<IObservable<PdfDocument>> pdfObservableFactory = () =>
                    fileSource.WhenAny(x => x.IsDownloaded, x => x.Value)
                    .Where(downhere => downhere == true)
                    .Take(1)
                    .SelectMany(_ => fileSource.File.GetFileFromCache(fileSource.Cache))
                    .SelectMany(stream => PdfDocument.LoadFromStreamAsync(stream))
                    .Catch<PdfDocument, Exception>(ex =>
                    {
                        Debug.WriteLine("The PDF rendering failed: {0}", ex.Message);
                        return Observable.Empty<PdfDocument>();
                    })
                    .PublishLast().ConnectAfterSubscription();

            // Finally, build the combination of these two guys.
            // Make sure that we don't keep re-creating this. We want to make sure
            // that only one version of the file (from pdfObservableFactory) is
            // generated. So do a Publish at the end here.
            var ck = cacheKey
                .Select(key => Tuple.Create(key, pdfObservableFactory())).Replay(1);
            _pdfAndCacheKey = ck;

            // The number of pages is complex in that we will need to fetch the file and render it if we've not already
            // cached it.
            Func<IObservable<PdfDocument>, IObservable<int>> fetchNumberOfPages = docs => docs.Select(d => (int)d.PageCount);
            _pdfAndCacheKey
                .SelectMany(info => fileSource.Cache.GetOrFetchObject(string.Format("{0}-NumberOfPages", info.Item1),
                                    () => fetchNumberOfPages(info.Item2),
                                    DateTime.Now + Settings.CacheFilesTime))
                .ToProperty(this, x => x.NumberOfPages, out _nPages, 0);

            // TODO: this should probably be a RefCount - otherwise this right here causes fetches
            // from all sorts of places (like the cache). Won't trigger a download, so it isn't too bad.
            ck.Connect();
        }

        /// <summary>
        /// Hold onto the file and cache key stream.
        /// </summary>
        private IObservable<Tuple<string, IObservable<PdfDocument>>> _pdfAndCacheKey;
    }
}
