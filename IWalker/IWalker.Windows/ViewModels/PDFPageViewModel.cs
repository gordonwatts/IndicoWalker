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

            // If they change how we do the rendering, then we will need to alter the actual width and height of the image we
            // are attached to.
            var altered = this.WhenAny(x => x.RenderWidth, x => x.RenderHeight, x => x.RenderingPriority, (x, y, z) => Tuple.Create(z.Value, x.Value, y.Value));
            altered
                .Where(trp => trp.Item1 == RenderingDimension.Horizontal && trp.Item2 > 0)
                .Select(trp => _page.Size.Height / _page.Size.Width * trp.Item2)
                .Distinct()
                .Subscribe(newHeight => RenderHeight = newHeight);
            altered
                .Where(trp => trp.Item1 == RenderingDimension.Vertical && trp.Item3 > 0)
                .Select(trp => _page.Size.Width / _page.Size.Height * trp.Item3)
                .Distinct()
                .Subscribe(newWidth => RenderWidth = newWidth);

            // Render the image at a certain width and height
            this.WhenAny(x => x.RenderWidth, x => x.RenderHeight, (x, y) => Tuple.Create(x.Value, y.Value))
                .Where(w => w.Item1 > 0 && w.Item2 > 0)
                .Distinct()
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
