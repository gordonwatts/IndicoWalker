using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Windows.Data.Pdf;
using Windows.UI.Xaml.Media.Imaging;

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
            Debug.Assert(page != null);
            _page = page;

            // If there is a rendering request, create the appropriate frame given our PDF page.
            RenderImage = ReactiveCommand.Create();
            var doRender = RenderImage
                .Cast<Tuple<RenderingDimension, double, double>>()
                .Select(t => CalcRenderingSize(t.Item1, t.Item2, t.Item2))
                .Where(d => d != null);

            // Now, make sure it is an ok rendering request.
            var newSize = doRender
                .Select(trp => Tuple.Create((int) trp.Item1, (int) trp.Item2))
                .DistinctUntilChanged()
                .Where(trp => trp.Item1 > 0 && trp.Item2 > 0);

            // The new size should be sent out immediately.
            UpdateImageSize = newSize;

            // Ok, rendering. We should start that only after things have settled just a little bit.
            newSize
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(500))
                .SelectMany(async szPixels =>
                {
                    var ms = new MemoryStream();
                    var ra = ms.AsRandomAccessStream();
                    var opt = new PdfPageRenderOptions() { DestinationWidth = (uint)szPixels.Item1, DestinationHeight = (uint)szPixels.Item2 };
                    Debug.WriteLine("Rendering PDF page {2} ({0} by {1})", opt.DestinationWidth, opt.DestinationHeight, _page.Index);
                    await _page.RenderToStreamAsync(ra, opt);
                    return ms;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(async ms =>
                {
                    var bm = new BitmapImage();
                    await bm.SetSourceAsync(ms.AsRandomAccessStream());
                    ms.Dispose();
                    return bm;
                })
                .ToProperty(this, x => x.Image, out _image, null, RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Calculate the size of the rendering area given the "expected" size (or room, frankly).
        /// </summary>
        /// <param name="orientation">Which dimension should we respect - where can we expand?</param>
        /// <param name="width">Width of the area, or 0 or infinity</param>
        /// <param name="height">Height of the area, or 0 or infinity</param>
        /// <returns></returns>
        public Tuple<int, int> CalcRenderingSize (RenderingDimension orientation, double width, double height)
        {
            switch (orientation)
            {
                case RenderingDimension.Horizontal:
                    if (width > 0)
                        return Tuple.Create((int) width, (int) (_page.Size.Height / _page.Size.Width * width));
                    return null;

                case RenderingDimension.Vertical:
                    if (height > 0)
                        return Tuple.Create((int) (_page.Size.Width / _page.Size.Height * height), (int) height);
                    return null;

                case RenderingDimension.MustFit:
                    if (width > 0 && height > 0)
                        return Tuple.Create((int) width, (int) height);
                    return null;

                default:
                    Debug.Assert(false);
                    return null;
            }
        }
    }
}
