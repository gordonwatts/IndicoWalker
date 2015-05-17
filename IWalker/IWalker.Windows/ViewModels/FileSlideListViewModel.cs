
using IWalker.Util;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
namespace IWalker.ViewModels
{
    /// <summary>
    /// This ViewModel renders the slides as a list of thumb nails (each thumb is managed by SlideThumbViewModel).
    /// It is usually viewed in the on the main app's meeting page, listing all the slide images one after the other.
    /// </summary>
    public class FileSlideListViewModel : ReactiveObject
    {
        /// <summary>
        /// The list of thumbnails
        /// </summary>
        public ReactiveList<SlideThumbViewModel> SlideThumbnails { get; private set; }

        /// <summary>
        /// Setup the VM for the file.
        /// </summary>
        /// <param name="downloader">The download controller. This assumes this is for a PDF file, and it is Valid.</param>
        /// <param name="talkTime">The time that this guy is relevant</param>
        public FileSlideListViewModel(FileDownloadController downloader, TimePeriod talkTime)
        {
            Debug.Assert(downloader != null);

            // Get the object consistent.
            SlideThumbnails = new ReactiveList<SlideThumbViewModel>();

            // We will want to refresh the view of this file depending on how close we are to the actual
            // meeting time.

            var innerBuffer = new TimePeriod(talkTime);
            innerBuffer.StartTime -= TimeSpan.FromMinutes(30);
            innerBuffer.EndTime += TimeSpan.FromHours(2);

            var updateTalkFile = Observable.Empty<Unit>();
            if (innerBuffer.Contains(DateTime.Now))
            {
                // Fire every 15 minutes, but only while in the proper time.
                // We only check when requested, so we will start right off the bat.
                updateTalkFile = Observable.Return(default(Unit))
                    .Concat(Observable.Interval(TimeSpan.FromMinutes(15))
                        .Where(_ => innerBuffer.Contains(DateTime.Now))
                        .Select(_ => default(Unit))
                    )
                    .Where(_ => Settings.AutoDownloadNewMeeting);
            }

            // The last trick is that if the file hasn't been downloaded, then we don't want to fire this.
            // This prevents this from being auto-downloaded, if the person has not set auto-download.

            updateTalkFile = updateTalkFile
                .Where(_ => downloader.IsDownloaded);

            // Ping the downloader to control when it should try to download things.

            updateTalkFile
                .InvokeCommand(downloader.DownloadOrUpdate);

            // Now attach a PDF file to this guy, which we can then use to get at all files.
            var pdfFile = new PDFFile(downloader);

            // A view model to show the whole thing as a strip view.
            var fullVM = new Lazy<FullTalkAsStripViewModel>(() => new FullTalkAsStripViewModel(Locator.Current.GetService<IScreen>(), pdfFile));

            // All we do is sit and watch for the # of pages to change, and when it does, we fix up the list of SlideThumbViewModel.
            pdfFile.WhenAny(x => x.NumberOfPages, x => x.Value)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(n => SetNumberOfThumbNails(n, pdfFile, fullVM));
        }

        /// <summary>
        /// Something about the # of pages in the list has changed. We need
        /// to update the list.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="pdfFile"></param>
        private async Task SetNumberOfThumbNails(int n, PDFFile pdfFile, Lazy<FullTalkAsStripViewModel> fullVM)
        {
            // If we are adding slides, make sure they are "setup" before showing them.
            if (SlideThumbnails.Count < n)
            {
                var newSlides = Enumerable.Range(SlideThumbnails.Count, n - SlideThumbnails.Count).Select(i => new SlideThumbViewModel(pdfFile.GetPageStreamAndCacheInfo(i), fullVM, i));
                foreach (var sld in newSlides)
                {
                    await sld.LoadSize();
                }
                SlideThumbnails.AddRange(newSlides);
            }
            else
            {
                while (SlideThumbnails.Count > n)
                {
                    SlideThumbnails.RemoveAt(SlideThumbnails.Count - 1);
                }

            }
        }
    }
}
