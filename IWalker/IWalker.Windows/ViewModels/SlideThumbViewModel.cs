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
        /// The PDF page that we are responsible for.
        /// </summary>
        private PdfPage _page;

        /// <summary>
        /// The image we are going to use for the bitmap.
        /// </summary>
        public BitmapImage Image
        {
            get { return _image.Value; }
        }
        private ObservableAsPropertyHelper<BitmapImage> _image = null;


        /// <summary>
        /// Set to the the width of the control.
        /// </summary>
        public double RenderWidth
        {
            get { return _renderWidth; }
            set { this.RaiseAndSetIfChanged(ref _renderWidth, value); }
        }
        private double _renderWidth;

        /// <summary>
        /// Initialize with the page that we should track.
        /// </summary>
        /// <param name="p"></param>
        public SlideThumbViewModel(Windows.Data.Pdf.PdfPage p)
        {
            _page = p;
            _renderWidth = 100;

            // When we arrive, prep the page for rendering on a background thread
            var prepForRender = ReactiveCommand.CreateAsyncTask(_ => _page.PreparePageAsync().AsTask());
            prepForRender.ExecuteAsync().Subscribe();

            // Render the image at a certian width
            var ms = new MemoryStream();
            var ra = ms.AsRandomAccessStream();
            this.WhenAny(x => x.RenderWidth, x => x.Value)
                .Where(w => w > 10)
                .SelectMany(async _ =>
                {
                    await _page.RenderToStreamAsync(ra, new PdfPageRenderOptions() { DestinationWidth = 100 });
                    var bm = new BitmapImage();
                    await bm.SetSourceAsync(ra);
                    return bm;
                })
                .ToProperty(this, x => x.Image, out _image, null, RxApp.MainThreadScheduler);
        }
    }
}
