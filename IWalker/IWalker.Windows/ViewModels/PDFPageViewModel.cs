using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Represents an image that is all or part of a PDF page.
    /// This guy supports:
    /// - Whatever size the image is, that is the size that is rendered
    /// </summary>
    /// <remarks>
    /// It would be nice to support:
    /// - Only render the portion visible
    /// </remarks>
    public class PDFPageViewModel : ReactiveObject
    {
        /// <summary>
        /// The PDF page that we are responsible for.
        /// </summary>
        private PdfPage _page;

        /// <summary>
        /// The image we are going to use for the display control. We will
        /// render to this guy.
        /// </summary>
        public BitmapImage Image
        {
            get { return _image.Value; }
        }
        private ObservableAsPropertyHelper<BitmapImage> _image = null;

        /// <summary>
        /// Fired by us at the end of the image render with the new image size.
        /// </summary>
        public IObservable<Tuple<int, int>> UpdateImageSize { get; private set; }

        /// <summary>
        /// Which dimension is important?
        /// Horizontal: The height is under our control and will be set to fit (perfectly) the PDF page for each page.
        /// Vertical: The width is under our control and will be set to fit (perfectly) the PDF page for each page.
        /// MustFit: Neither is under our control, and is set externally. We will make sure the image renders in the control completely.
        /// </summary>
        public enum RenderingDimension
        {
            Horizontal,
            Vertical,
            MustFit
        };

        /// <summary>
        /// This is executed in order to trigger a rendering. It should 
        /// contain the rendering mode, along with the x and y size of the
        /// space that we are going to render into.
        /// </summary>
        public ReactiveCommand<object> RenderImage { get; private set; }

        /// <summary>
        /// Initialize with the page that we should track.
        /// </summary>
        /// <param name="page">Page to render</param>
        /// <remarks>We do not prepare the PDF document for rendering ahead of time (calling PreparePageAsync)</remarks>
        public PDFPageViewModel(PdfPage page)
        {
            _page = page;

            // If there is a rendering request, create the appropriate frame given our PDF page.
            RenderImage = ReactiveCommand.Create();
            var doRender = RenderImage
                .Cast<Tuple<RenderingDimension, double, double>>();
            var hRender = doRender
                .Where(trp => trp.Item1 == RenderingDimension.Horizontal && trp.Item2 > 0)
                .Select(trp => Tuple.Create(trp.Item2, _page.Size.Height / _page.Size.Width * trp.Item2));
            var vRender = doRender
                .Where(trp => trp.Item1 == RenderingDimension.Vertical && trp.Item3 > 0)
                .Select(trp => Tuple.Create(trp.Item2, _page.Size.Width / _page.Size.Height * trp.Item3));
            var fRender = doRender
                .Where(trp => trp.Item1 == RenderingDimension.MustFit)
                .Select(trp => Tuple.Create(trp.Item2, trp.Item3));

            // Now, make sure it is an ok rendering request.
            var newSize = Observable.Merge(hRender, vRender, fRender)
                .Select(trp => Tuple.Create((int) trp.Item1, (int) trp.Item2))
                .Distinct()
                .Where(trp => trp.Item1 > 0 && trp.Item2 > 0);

            // The new size should be sent out immediately.
            UpdateImageSize = newSize;

            // Ok, rendering. We should start that only after things have settled just a little bit.
            newSize
                .Throttle(TimeSpan.FromMilliseconds(500))
                .SelectMany(async szPixels =>
                {
                    var ms = new MemoryStream();
                    var ra = ms.AsRandomAccessStream();
                    var opt = new PdfPageRenderOptions() { DestinationWidth = (uint)szPixels.Item1, DestinationHeight = (uint)szPixels.Item2 };
                    Debug.WriteLine("Rendering PDF page ({0} by {1})", opt.DestinationWidth, opt.DestinationHeight);
                    await _page.RenderToStreamAsync(ra, opt);
                    return ms;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(async ms =>
                {
                    var bm = new BitmapImage();
                    await bm.SetSourceAsync(ms.AsRandomAccessStream());
                    return bm;
                })
                .ToProperty(this, x => x.Image, out _image, null, RxApp.MainThreadScheduler);
        }
    }
}
