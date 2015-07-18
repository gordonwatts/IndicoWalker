
using IWalker.Util;
using ReactiveUI;
using Splat;
using System;
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
        public FileSlideListViewModel(PDFFile pdfFile, TimePeriod talkTime)
        {
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

            // A view model to show the whole thing as a strip view.
            var fullVM = new Lazy<FullTalkAsStripViewModel>(() => new FullTalkAsStripViewModel(Locator.Current.GetService<IScreen>(), pdfFile));

            // All we do is sit and watch for the # of pages to change, and when it does, we fix up the list of SlideThumbViewModel.

            var newSlideInfo = from nPages in pdfFile.WhenAny(x => x.NumberOfPages, x => x.Value).DistinctUntilChanged()
                               from newSlides in CreateNewThumbs(nPages, pdfFile, fullVM)
                               select new
                               {
                                   n = nPages,
                                   slides = newSlides
                               };

            newSlideInfo
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(info => SetNumberOfThumbNails(info.n, info.slides));
        }

        /// <summary>
        /// Update the UI list of page renderings.
        /// </summary>
        /// <param name="n">Number of pages</param>
        /// <remarks>Must run on UI thread</remarks>
        /// <param name="slideThumbViewModel">New slides to be added (might be an empty array)</param>
        private void SetNumberOfThumbNails(int n, SlideThumbViewModel[] slideThumbViewModel)
        {
            SlideThumbnails.AddRange(slideThumbViewModel);
            while (SlideThumbnails.Count > n)
            {
                SlideThumbnails.RemoveAt(SlideThumbnails.Count - 1);
            }
        }

        /// <summary>
        /// Create the new slides and load them up.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="pdfFile"></param>
        /// <param name="fullVM"></param>
        /// <returns></returns>
        private async Task<SlideThumbViewModel[]> CreateNewThumbs(int n, PDFFile pdfFile, Lazy<FullTalkAsStripViewModel> fullVM)
        {
            var newSlides = Enumerable.Range(SlideThumbnails.Count, n - SlideThumbnails.Count).Select(i => new SlideThumbViewModel(pdfFile.GetPageStreamAndCacheInfo(i), fullVM, i)).ToArray();
            foreach (var sld in newSlides)
            {
                await sld.LoadSize();
            }
            return newSlides;
        }
    }
}
