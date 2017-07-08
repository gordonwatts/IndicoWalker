using IWalker.Util;
using ReactiveUI;
using System.Reactive.Linq;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Presents a button. When pressed, it expands so all the slides are shown as thumbnails.
    /// Only a limited number are allowed to be open at any time.
    /// </summary>
    public class ExpandingSlideThumbViewModel : ReactiveObject
    {
        /// <summary>
        /// When fired, it cause the strip of slides to be populated.
        /// </summary>
        public ReactiveCommand<object> ShowSlides { get; private set; }

        /// <summary>
        /// Now many slides are available for this talk?
        /// </summary>
        public int NumberOfSlides
        {
            get { return _numberOfSlides.Value; }
        }
        private ObservableAsPropertyHelper<int> _numberOfSlides;

        /// <summary>
        /// True if we are able to show the thumbnails (and aren't currently).
        /// </summary>
        public bool CanShowThumbs
        {
            get { return _canShowThumbs.Value; }
        }
        private ObservableAsPropertyHelper<bool> _canShowThumbs;

        /// <summary>
        /// Fired to close all other slide shows.
        /// </summary>
        private static ReactiveCommand<object> _resetSlideShow = ReactiveCommand.Create();

        /// <summary>
        /// Set to null mostly, but when not, it contains the VM for the file slides.
        /// </summary>
        public FileSlideListViewModel TalkAsThumbs { get { return _talkAsThumbs.Value; } }
        private ObservableAsPropertyHelper<FileSlideListViewModel> _talkAsThumbs;

        /// <summary>
        /// Setup the links to run.
        /// </summary>
        public ExpandingSlideThumbViewModel(PDFFile downloader, TimePeriod talkTime)
        {
            // Showing the slides should generate it here, and nullify it everywhere else.
            ShowSlides = ReactiveCommand.Create();

            // FIre the reset command, with the current downloader as an argument
            ShowSlides
                .Select(_ => downloader)
                .InvokeCommand(_resetSlideShow);

            // When the reset shows up, only pay attention if a different file is being shown
            // and we have something to show!
            var noThumbs = _resetSlideShow
                .Where(dl => downloader != dl && downloader.NumberOfPages != 0)
                .Select(_ => (FileSlideListViewModel)null);

            // When we want to show the slides just create the new view model.
            var newThumbs = ShowSlides
                .Select(_ => new FileSlideListViewModel(downloader, talkTime));

            // Now, allow the property we manage to flip back and forth here.
            Observable.Merge(newThumbs, noThumbs)
                .WriteLine(x => string.Format("--> writing out a {0} to the actual display property.", x == null ? "null" : "non-null"))
                .ToProperty(this, x => x.TalkAsThumbs, out _talkAsThumbs, null, RxApp.MainThreadScheduler);

            // The logic for canShowThumbs is tricky:
            // 1. True if the downloader has downloaded and opened the file and parsed it successfully (# pages != 0).
            // 2. False if we have fired off a new thumbs guy, but that can be only active if can show thumbs is true (right??).
            // 3. True if we've shown thumbs and we stop showing them.

            var downloaded = downloader.WhenAny(x => x.NumberOfPages, x => x.Value != 0);
            var areShowing = Observable.Merge(newThumbs, noThumbs)
                .Select(x => x == null ? true : false);

            Observable.Merge(downloaded, areShowing)
                .WriteLine(x => string.Format("  -> and writing out {0} for can show thumbs", x))
                .ToProperty(this, x => x.CanShowThumbs, out _canShowThumbs, false, RxApp.MainThreadScheduler);

            // Track the # of pages. Used to display some info below the button in most implementations.
            downloader.WhenAny(x => x.NumberOfPages, x => x.Value)
                .ToProperty(this, x => x.NumberOfSlides, out _numberOfSlides, 0, RxApp.MainThreadScheduler);

        }
    }
}
