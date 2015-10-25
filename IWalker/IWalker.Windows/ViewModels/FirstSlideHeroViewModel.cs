using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Given a PDF file, display a hero slide which is the first slide. Take up no space and
    /// no thought if we aren't given something to show. :-)
    /// </summary>
    public class FirstSlideHeroViewModel : ReactiveObject
    {
        /// <summary>
        /// Get the page VM.
        /// </summary>
        /// <remarks>Null if we don't have anything to show</remarks>
        public PDFPageViewModel HeroPageUC { get { return _heroPageUC != null ? _heroPageUC.Value : null; } }
        private ObservableAsPropertyHelper<PDFPageViewModel> _heroPageUC;

        /// <summary>
        /// Returns true if we have a hero slide to be shown.
        /// </summary>
        public bool HaveHeroSlide { get; private set; }
        private ObservableAsPropertyHelper<bool> _haveHeroSlide;

        /// <summary>
        /// Open the full view of the talk.
        /// </summary>
        public ReactiveCommand<object> OpenFullView { get; set; }

        /// <summary>
        /// Create the VM. We generate a first page hero slide.
        /// </summary>
        /// <param name="file">The PDF File to generate. If null, we will make this VM as invalid</param>
        public FirstSlideHeroViewModel(PDFFile file, Lazy<FullTalkAsStripViewModel> fullVM)
        {
            // If we are actually connected to a file, then
            // - setup the hero slide
            // - a button to show all slides
            _heroPageUC = null;
            HaveHeroSlide = false;
            if (file != null)
            {
                // Hero slide. Tricky because we can't display until
                // a fetch has been done on the PDF data.
                var pdf = new PDFPageViewModel(file.GetPageStreamAndCacheInfo(0));
                _heroPageUC = pdf.LoadSize()
                    .Select(_ => pdf)
                    .ToProperty(this, m => m.HeroPageUC, scheduler: RxApp.MainThreadScheduler);

                HaveHeroSlide = true;

                // Allow a full view
                OpenFullView = ReactiveCommand.Create();
                OpenFullView
                    .Subscribe(_ => fullVM.Value.LoadPage(0));
            }
        }

        /// <summary>
        /// Create the VM starting from a file downloader
        /// </summary>
        /// <param name="fileDownloader"></param>
        public FirstSlideHeroViewModel(FileDownloadController fileDownloader, Lazy<FullTalkAsStripViewModel> fullVM)
            : this(fileDownloader == null ? null : new PDFFile(fileDownloader), fullVM)
        {
        }
    }
}
