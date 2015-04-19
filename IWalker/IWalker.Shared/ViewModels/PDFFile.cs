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
        /// Mostly for testing, fires when we have a new PDF document ready
        /// to be looked at.
        /// </summary>
        public IObservable<Unit> PDFDocumentUpdated { get; private set; }

        /// <summary>
        /// Return a stream of PDF pages, which will be updated each time
        /// the file is updated.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public IObservable<PdfPage> GetPageStream(int pageNumber)
        {
            throw new InvalidOperationException();
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
            var pdfDocument = Observable.Merge(isDownloaded, newFile)
                .SelectMany(_ => fileSource.Cache.GetObjectCreatedAt<Tuple<string, byte[]>>(fileSource.File.UniqueKey))
                .DistinctUntilChanged(info => info.Value)
                .SelectMany(info => fileSource.File.GetFileFromCache(fileSource.Cache))
                .SelectMany(stream => PdfDocument.LoadFromStreamAsync(stream))
                .Do(doc => Debug.WriteLine("Done with document initial rendering"))
                .Catch<PdfDocument, Exception>(ex => Observable.Empty<PdfDocument>())
                .Publish();

            // Now we can parcel out that information.

            pdfDocument
                .Select(doc => (int)doc.PageCount)
                .ToProperty(this, x => x.NumberOfPages, out _nPages, 0, RxApp.MainThreadScheduler);


            pdfDocument.Connect();
        }
    }
}
