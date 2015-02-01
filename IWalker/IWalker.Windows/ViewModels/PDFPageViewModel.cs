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
        /// Hold onto a weak reference to the above image. This will enable us
        /// to keep it even if we don't really need it but the system doesn't
        /// want it back.
        /// </summary>
        private WeakReference<BitmapImage> _weakReferenceToImage = null;

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
        /// Get/Set the state of the attached image. False means that the image reference is set to null, but a weak reference
        /// will be kept (if the image was already rendered). True means the image will be rendered and kept.
        /// </summary>
        public bool AttachImage
        {
            get { return _showImage; }
            set
            {
                this.RaiseAndSetIfChanged(ref _showImage, value);
            }
        }
        bool _showImage = true;

        /// <summary>
        /// Hold onto the last rendered size. We will use this cache if we have to
        /// re-render for some reason.
        /// </summary>
        private Tuple<int, int> _lastRequestedRenderSize;

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
            var renderRequest = RenderImage
                .Cast<Tuple<RenderingDimension, double, double>>()
                .Select(t => CalcRenderingSize(t.Item1, t.Item2, t.Item2))
                .Where(d => d != null);

            // Now, make sure it is an ok rendering request.
            var newSize = renderRequest
                .Select(trp => Tuple.Create((int)trp.Item1, (int)trp.Item2))
                .DistinctUntilChanged();

            // Ok, rendering. We should start that only after things have settled just a little bit.
            var newRender = newSize
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(500));

            // Cache the info so that if we have to re-build on the fly we can.
            newRender
                .Subscribe(t =>
                {
                    _weakReferenceToImage = null;
                    _lastRequestedRenderSize = t;
                });

            // The weak reference logic. We need to be able to release the image when
            // it isn't needed, and then re-use it. If the image has been garbage collected,
            // then we also need to re-render it if we need to re-use it.
            var reusableImage = this.WhenAny(x => x.AttachImage, x => x.Value)
                .Where(show => show)
                .Select(t =>
                {
                    BitmapImage img = null;
                    if (_weakReferenceToImage == null || !_weakReferenceToImage.TryGetTarget(out img))
                    {
                        return null;
                    }
                    return img;
                });
            var resueImage = reusableImage
                .Where(img => img != null);
            var reRenderImage = reusableImage
                .Where(img => img == null)
                .Where(i => _lastRequestedRenderSize != null)
                .Select(i => _lastRequestedRenderSize);
            Tuple<int, int> myv;
            reRenderImage
                .Subscribe(t => myv = t);

            // When the image isn't really needed, update as we need.
            var eraseImage = this.WhenAny(x => x.AttachImage, x => x.Value)
                .Where(show => !show)
                .Do(show => Debug.WriteLine("Going to release the image link for page {0}", _page.Index))
                .Select(t => (BitmapImage)null);

            // Do the actual rendering.
            // TODO: Can we clean this code up (and others places) by using Observable.Merge rather than this initial chaining? It would make the code more clear.
            var newImage = newRender
                .Merge(reRenderImage)
                .Do(t => _weakReferenceToImage = null)
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
                    _weakReferenceToImage = new WeakReference<BitmapImage>(bm);
                    return bm;
                });

            // Save all image changes so the UI knows to update!
            newImage
                .Merge(resueImage)
                .Merge(eraseImage)
                .ToProperty(this, x => x.Image, out _image, null, RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Calculate the size of the rendering area given the "expected" size (or room, frankly).
        /// </summary>
        /// <param name="orientation">Which dimension should we respect - where can we expand?</param>
        /// <param name="width">Width of the area, or 0 or infinity</param>
        /// <param name="height">Height of the area, or 0 or infinity</param>
        /// <returns></returns>
        public Tuple<int, int> CalcRenderingSize(RenderingDimension orientation, double width, double height)
        {
            switch (orientation)
            {
                case RenderingDimension.Horizontal:
                    if (width > 0)
                        return Tuple.Create((int)width, (int)(_page.Size.Height / _page.Size.Width * width));
                    return null;

                case RenderingDimension.Vertical:
                    if (height > 0)
                        return Tuple.Create((int)(_page.Size.Width / _page.Size.Height * height), (int)height);
                    return null;

                case RenderingDimension.MustFit:
                    if (width > 0 && height > 0)
                        return Tuple.Create((int)width, (int)height);
                    return null;

                default:
                    Debug.Assert(false);
                    return null;
            }
        }
    }
}
