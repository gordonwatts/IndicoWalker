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
        /// The internal view model for the page.
        /// </summary>
        public PDFPageViewModel PDFPageVM { get; private set; }

        /// <summary>
        /// Initialize with the page that we should track.
        /// </summary>
        /// <param name="page">The PDF page to be rendered</param>
        /// <remarks>We will call PeparePageAsync on the page</remarks>
        public SlideThumbViewModel(PdfPage page)
        {
            PDFPageVM = new PDFPageViewModel(page);

            // Prepare the slide for rendering.
            var prepForRender = ReactiveCommand.CreateAsyncTask(_ => page.PreparePageAsync().AsTask());
            prepForRender.ExecuteAsync().Subscribe();
        }
    }
}
