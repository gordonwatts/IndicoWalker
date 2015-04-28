using Akavache;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
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

            RenderImage = ReactiveCommand.Create();
            var renderRequest = RenderImage
                .Cast<Tuple<RenderingDimension, double, double>>()
                .Where(info => (info.Item1 == RenderingDimension.Horizontal && info.Item2 > 0)
                    || (info.Item1 == RenderingDimension.Vertical && info.Item3 > 0)
                    || (info.Item1 == RenderingDimension.MustFit && info.Item2 > 0 && info.Item3 > 0)
                );

            ImageStream =
                Observable.CombineLatest(pageInfo, renderRequest, (pinfo, rr) => Tuple.Create(pinfo.Item1, pinfo.Item2, rr.Item1, rr.Item2, rr.Item3))
                .SelectMany(info => _cache.GetOrFetchObject<IWalkerSize>(MakeSizeCacheKey(info.Item1),
                    () => info.Item2.Take(1).Select(pdf => pdf.Size.ToIWalkerSize()),
                    DateTime.Now + Settings.PageCacheTime)
                    .Select(sz => Tuple.Create(info.Item1, info.Item2, CalcRenderingSize(info.Item3, info.Item4, info.Item5, sz))))
                .SelectMany(info => _cache.GetOrFetchObject<byte[]>(MakePageCacheKey(info.Item1, info.Item3),
                    () => info.Item2.SelectMany(pdfPg =>
                    {
                        var ms = new MemoryStream();
                        var ra = WindowsRuntimeStreamExtensions.AsRandomAccessStream(ms);
                        var opt = new PdfPageRenderOptions() { DestinationWidth = (uint)info.Item3.Item1, DestinationHeight = (uint)info.Item3.Item2 };
                        return Observable.FromAsync(() => pdfPg.RenderToStreamAsync(ra).AsTask())
                            .Select(_ => ms.ToArray());
                    }),
                    DateTime.Now + Settings.PageCacheTime
                    ))
                .Select(bytes => new MemoryStream(bytes));
        }

        /// <summary>
        /// Create an image cache key
        /// </summary>
        /// <param name="pageCacheKey"></param>
        /// <param name="renderSize"></param>
        /// <returns></returns>
        private string MakePageCacheKey(string pageCacheKey, Tuple<int, int> renderSize)
        {
            return string.Format("{0}-w{1}-h{2}", pageCacheKey, renderSize.Item1, renderSize.Item2);
        }

        /// <summary>
        /// We need to cache the size of the page so we don't need the actual PDF page.
        /// </summary>
        /// <param name="normalCacheKey"></param>
        /// <returns></returns>
        private string MakeSizeCacheKey(string normalCacheKey)
        {
            return normalCacheKey + "-DefaultPageSize";
        }

        /// <summary>
        /// Generate the cache key from the data. This should be unique enough such that
        /// when the file changes or similar the cache hit will fail.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="cacheTag"></param>
        /// <returns></returns>
        private string MakeCacheKey(uint pageNumber, int width, int height, string cacheTag)
        {
            return string.Format("{0}-{1}-{2}x{3}", cacheTag, pageNumber, width, height);
        }

        /// <summary>
        /// Calculate the size of the rendering area given the "expected" size (or room, frankly).
        /// </summary>
        /// <param name="orientation">Which dimension should we respect - where can we expand?</param>
        /// <param name="width">Width of the area, or 0 or infinity</param>
        /// <param name="height">Height of the area, or 0 or infinity</param>
        /// <returns></returns>
        public Tuple<int, int> CalcRenderingSize(RenderingDimension orientation, double width, double height, IWalkerSize pageSize)
        {
            switch (orientation)
            {
                case RenderingDimension.Horizontal:
                    if (width > 0)
                        return Tuple.Create((int)width, (int)(pageSize.Height / pageSize.Width * width));
                    return null;

                case RenderingDimension.Vertical:
                    if (height > 0)
                        return Tuple.Create((int)(pageSize.Width / pageSize.Height * height), (int)height);
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
