using Akavache;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Windows.Data.Pdf;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Represents a single page in a PDF file.
    /// This guy supports:
    ///   - Whatever size the image is, that is the size that is rendered
    ///   - Caching of images already rendered (so they don't have to be re-rendered).
    ///   - If the page is updated after we are up, update the image.
    /// </summary>
    /// <remarks>
    /// It would be nice to support:
    /// - Only render the portion visible
    /// - Support zooming
    /// </remarks>
    public class PDFPageViewModel : ReactiveObject
    {
        private IBlobCache _cache;
        /// <summary>
        /// The image we are going to use for the display control. We will
        /// render to this guym, and send a new one (or an old one) each time.
        /// No rendering will occur unless this guy is subscribed to.
        /// </summary>
        public IObservable<MemoryStream> ImageStream { get; private set; }

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
        /// Fires when a page size is loaded.
        /// </summary>
        private IObservable<Unit> _pageSizeLoaded = null;

        /// <summary>
        /// Returns a sequence that when complete indicates the image
        /// size has been properly loaded and all the sizing methods can be called
        /// and will return a valid value.
        /// </summary>
        /// <returns>Sequence that will fire just once before terminating after this object has a valid page size</returns>
        public IObservable<Unit> LoadSize()
        {
            return _pageSizeLoaded.Take(1);
        }

        /// <summary>
        /// Track the size of this page. Loaded from cache or from the PdfPage we go and fetch.
        /// </summary>
        private IWalkerSize _pageSize = null;

        /// <summary>
        /// Initialize with the page that we should track.
        /// </summary>
        /// <param name="pageInfo">A stream of cache tags and the PDF pages associated with it.</param>
        /// <remarks>We are really only interested in the first PdfPage we get - and we will re-subscribe, but not hold onto it.</remarks>
        public PDFPageViewModel(IObservable<Tuple<string, IObservable<PdfPage>>> pageInfo, IBlobCache cache = null)
        {
            _cache = cache ?? Blobs.LocalStorage;

            // Render an image when:
            //   - We have at least one render request
            //   - ImageStream is subscribed to
            //   - the file or page updates somehow (new pageInfo iteration).
            //
            // Always pull from the cache first, but if that doesn't work, then render and place
            // in the cache.

            // Get the size of the thing when it is requested. This sequence must be finished before
            // any size calculations can be done!
            var imageSize = from pgInfo in pageInfo
                            from sz in _cache.GetOrFetchPageSize(pgInfo.Item1, () => pgInfo.Item2.Take(1).Select(pdf => pdf.Size.ToIWalkerSize()))
                            select new
                            {
                                PGInfo = pgInfo,
                                Size = sz
                            };
            var publishedSize = imageSize
                .Do(info => _pageSize = info.Size)
                .Select(info => info.PGInfo)
                .Publish().RefCount();

            _pageSizeLoaded = publishedSize.AsUnit();

            // The render request must be "good" - that is well formed. Otherwise
            // we will just ignore it. This is because sometimes when things are being "sized" the
            // render request is malformed.
            RenderImage = ReactiveCommand.Create();
            var renderRequest = RenderImage
                .Cast<Tuple<RenderingDimension, double, double>>()
                .Where(info => (info.Item1 == RenderingDimension.Horizontal && info.Item2 > 0)
                    || (info.Item1 == RenderingDimension.Vertical && info.Item3 > 0)
                    || (info.Item1 == RenderingDimension.MustFit && info.Item2 > 0 && info.Item3 > 0)
                );

            // Generate an image when we have a render request and a everything else is setup.
            ImageStream = from requestInfo in Observable.CombineLatest(publishedSize, renderRequest, (pSize, rr) => new { pgInfo = pSize, RenderRequest = rr })
                          let imageDimensions = CalcRenderingSize(requestInfo.RenderRequest.Item1, requestInfo.RenderRequest.Item2, requestInfo.RenderRequest.Item3)
                          from imageData in _cache.GetOrFetchPageImageData(requestInfo.pgInfo.Item1, imageDimensions.Item1, imageDimensions.Item2,
                          () => requestInfo.pgInfo.Item2.SelectMany(pdfPg =>
                                        {
                                            var ms = new MemoryStream();
                                            var ra = WindowsRuntimeStreamExtensions.AsRandomAccessStream(ms);
                                            var opt = new PdfPageRenderOptions() { DestinationWidth = (uint)imageDimensions.Item1, DestinationHeight = (uint)imageDimensions.Item2 };
                                            return Observable.FromAsync(() => pdfPg.RenderToStreamAsync(ra).AsTask())
                                                .Select(_ => ms.ToArray());
                                        }))
                          select new MemoryStream(imageData);
        }

        /// <summary>
        /// Calculate the size of the rendering area given the "expected" size (or room, frankly).
        /// </summary>
        /// <param name="orientation">Which dimension should we respect - where can we expand?</param>
        /// <param name="width">Width of the area, or 0 or infinity</param>
        /// <param name="height">Height of the area, or 0 or infinity</param>
        /// <returns></returns>
        /// <remarks>If this is called before the initialization sequence is done, then there will be a bomb below
        /// because we don't know the size to base our rendering on!</remarks>
        public Tuple<int, int> CalcRenderingSize(RenderingDimension orientation, double width, double height)
        {
            Debug.Assert(_pageSize != null);

            if (_pageSize == null)
            {
                return null;
            }
            switch (orientation)
            {
                case RenderingDimension.Horizontal:
                    if (width > 0)
                        return Tuple.Create((int)width, (int)(_pageSize.Height / _pageSize.Width * width));
                    return null;

                case RenderingDimension.Vertical:
                    if (height > 0)
                        return Tuple.Create((int)(_pageSize.Width / _pageSize.Height * height), (int)height);
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
