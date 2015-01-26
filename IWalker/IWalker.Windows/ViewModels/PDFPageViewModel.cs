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
        /// The size of the image that we are rendering.
        /// </summary>
        public double RenderWidth
        {
            get { return _renderWidth; }
            set { this.RaiseAndSetIfChanged(ref _renderWidth, value); }
        }
        private double _renderWidth;

        /// <summary>
        /// The size of the image that we are rendering.
        /// </summary>
        public double RenderHeight
        {
            get { return _renderHeight; }
            set { this.RaiseAndSetIfChanged(ref _renderHeight, value); }
        }
        private double _renderHeight;

        /// <summary>
        /// Which way around is the important dimension? We will make sure the image in that
        /// direction is exact, and the other dimension will be dictated by the page's aspect ratio.
        /// </summary>
        public enum RenderingDimension
        {
            Horizontal,
            Vertical,
            MustFit
        };

        /// <summary>
        /// Get or set the way we do the rendering.
        /// </summary>
        public RenderingDimension RenderingPriority
        {
            get { return _renderingPriority; }
            set { this.RaiseAndSetIfChanged(ref _renderingPriority, value); }
        }
        private RenderingDimension _renderingPriority;

        /// <summary>
        /// Initialize with the page that we should track.
        /// </summary>
        /// <param name="page">Page to render</param>
        /// <remarks>We do not prepare the PDF document for rendering ahead of time (calling PreparePageAsync)</remarks>
        public PDFPageViewModel(PdfPage page)
        {
            _page = page;

            // Render the image at a certain width
            this.WhenAny(x => x.RenderWidth, x => x.RenderHeight, x => x.RenderingPriority, (x, y, z) => Tuple.Create(x.Value, y.Value))
                .Where(w => NewDimensionsOK(w))
                .Throttle(TimeSpan.FromMilliseconds(500))
                .SelectMany(async szPixels =>
                {
                    var ms = new MemoryStream();
                    var ra = ms.AsRandomAccessStream();
                    await _page.RenderToStreamAsync(ra, MakeRenderingOptions(szPixels));
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

        /// <summary>
        /// Get the rendering options given our current setup.
        /// </summary>
        /// <param name="szPixels"></param>
        /// <returns></returns>
        private PdfPageRenderOptions MakeRenderingOptions(Tuple<double, double> szPixels)
        {
            switch (RenderingPriority)
            {
                case RenderingDimension.Horizontal:
                    return new PdfPageRenderOptions() { DestinationWidth = (uint)szPixels.Item1 };

                case RenderingDimension.Vertical:
                    return new PdfPageRenderOptions() { DestinationHeight = (uint)szPixels.Item2 };

                case RenderingDimension.MustFit:
                    return new PdfPageRenderOptions() { DestinationWidth = (uint)szPixels.Item1, DestinationHeight = (uint)szPixels.Item2 };

                default:
                    Debug.Assert(false);
                    return null;

            }
        }

        /// <summary>
        /// Given our settings, check to see if we have reasonable dimensions.
        /// </summary>
        /// <param name="w">Dimensions we should be checking</param>
        /// <returns></returns>
        private bool NewDimensionsOK(Tuple<double, double> w)
        {
            switch (RenderingPriority)
            {
                case RenderingDimension.Horizontal:
                    if (w.Item1 <= 0)
                        return false;
                    return true;

                case RenderingDimension.Vertical:
                    if (w.Item2 <= 0)
                        return false;
                    return true;

                case RenderingDimension.MustFit:
                    if (w.Item1 <= 0 || w.Item2 <= 0)
                        return false;
                    return true;

                default:
                    Debug.Assert(false);
                    return false;
            }
        }
    }
}
