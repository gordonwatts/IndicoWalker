using ReactiveUI;
using System;
using System.IO;
using System.Reactive.Linq;
using Windows.Data.Pdf;
using Windows.UI.Xaml.Media.Imaging;

namespace IWalker.ViewModels
{
    /// <summary>
    /// We hold onto a single image that is the thumbnail, and do the rendering for it.
    /// </summary>
    public class SlideThumbViewModel : ReactiveObject
    {
        /// <summary>
        /// Where we are going if the user clicks on this slide.
        /// </summary>
        /// <remarks>This is only created and allocated if it is required.</remarks>
        private Lazy<FullTalkAsStripViewModel> fullVM;

        /// <summary>
        /// The internal view model for the page.
        /// </summary>
        public PDFPageViewModel PDFPageVM { get; private set; }

        /// <summary>
        /// Execute to open up the full view of the talk as a strip, starting with this slide.
        /// </summary>
        public ReactiveCommand<object> OpenFullView { get; private set; }

        /// <summary>
        /// Initialize with the page that we should track.
        /// </summary>
        /// <param name="page">The PDF page to be rendered</param>
        /// <remarks>We will call PeparePageAsync on the page</remarks>
        public SlideThumbViewModel(PdfPage page, Lazy<FullTalkAsStripViewModel> fullVM, int pageNumber)
        {
            PDFPageVM = new PDFPageViewModel(page);
            this.fullVM = fullVM;

            // Prepare the slide for rendering.
            var prepForRender = ReactiveCommand.CreateAsyncTask(_ => page.PreparePageAsync().AsTask());
            prepForRender.ExecuteAsync().Subscribe();

            // And the command to open up a full view of the talk, at max size.
            OpenFullView = ReactiveCommand.Create();
            OpenFullView
                .Subscribe(_ => fullVM.Value.LoadPage(pageNumber));
        }
    }
}
