using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace IWalker.Views
{
    /// <summary>
    /// The view for a PDF page. We are a very simple image, with a bunch of wiring behind it.
    /// </summary>
    /// <remarks>
    public sealed partial class PDFPageUserControl : UserControl, IViewFor<PDFPageViewModel>
    {
        private PDFPageViewModel _pageVMCache;

        public PDFPageUserControl()
        {
            this.InitializeComponent();

            var gd = new CompositeDisposable();

            // Tie together the image source.
            // There are two ObserveOn's below, both are required:
            //   - BItmapImage must be dealt with on the main thread
            //   - Despite that, when it comes back from loading the image, it may not be on the default thread!
            // TODO: can we use this to make this cleaner? http://stackoverflow.com/questions/24049931/making-an-iobservablet-that-uses-async-await-return-completed-tasks-in-origina


            gd.Add(this.WhenAny(x => x.ViewModel.ImageStream, x => x.Value)
                .SelectMany(imageStreams => imageStreams)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(_ => _pageVMCache = ViewModel)
                .SelectMany(stream => ConvertToBMI(stream, _pageVMCache))
                .Where(t => t.Item2 == _pageVMCache)
                .Select(t => t.Item1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(ThumbImage, x => x.Source));

            // The following things should cause a re-rendering:
            // 1) The size of the control changes
            // 2) ShowPDF changes
            // 3) The VM we are connected to changes
            //    This can happen when the rendering system recycles this View from a cache in an attempt to keep memory
            //    usage low.

            gd.Add(this.Events().SizeChanged.Select(a => default(Unit))
                .Merge(this.WhenAny(x => x.ShowPDF, x => default(Unit)))
                .Where(_ => ShowPDF)
                .Select(_ => Tuple.Create(RespectRenderingDimension, ActualWidth, ActualHeight))
                .DistinctUntilChanged()
                .CombineLatest(this.WhenAny(x => x.ViewModel, x => x.Value).Where(x => x != null), (t, vm) => t)
                .Subscribe(t => ViewModel.RenderImage.Execute(t)));

            // Finally, when we are activated, provide a way to release all of our resources.
            this.WhenActivated(disposeOfMe =>
            {
                if (gd != null)
                {
                    disposeOfMe(gd);
                }
                gd = null;
            });
        }

        /// <summary>
        /// Convert a stream to an image, and dispose of the stream.
        /// This must be called on the UI thread!
        /// When the async is done, it may not do the callback on the UI thread!
        /// </summary>
        /// <param name="imageStream"></param>
        /// <returns></returns>
        private async Task<Tuple<BitmapImage, PDFPageViewModel>> ConvertToBMI(MemoryStream imageStream, PDFPageViewModel vm)
        {
            imageStream.Seek(0, SeekOrigin.Begin);
            var bm = new BitmapImage();
            await bm.SetSourceAsync(WindowsRuntimeStreamExtensions.AsRandomAccessStream(imageStream));
            imageStream.Dispose();
            return Tuple.Create(bm, vm);
        }

        /// <summary>
        /// If true, will render and show the PDF. If false, it won't be shown.
        /// </summary>
        /// <remarks>
        /// - When set to true, the control will render the page and display it.
        /// - When set to false, the image will not be displayed. And the image can be garbage collected.
        /// In all cases the control will have the correct size.
        /// </remarks>
        public bool ShowPDF
        {
            get { return (bool)GetValue(ShowPDFProperty); }
            set { SetValue(ShowPDFProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowPDF.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowPDFProperty =
            DependencyProperty.Register("ShowPDF", typeof(bool), typeof(PDFPageUserControl), new PropertyMetadata(true));

        /// <summary>
        /// Get/set the rendering dimension to respect.
        /// </summary>
        public PDFPageViewModel.RenderingDimension RespectRenderingDimension
        {
            get { return (PDFPageViewModel.RenderingDimension)GetValue(RespectRenderingDimensionProperty); }
            set { SetValue(RespectRenderingDimensionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RespectRenderingDimension.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RespectRenderingDimensionProperty =
            DependencyProperty.Register("RespectRenderingDimension", typeof(PDFPageViewModel.RenderingDimension), typeof(PDFPageUserControl), new PropertyMetadata(PDFPageViewModel.RenderingDimension.Horizontal));

        /// <summary>
        /// Track the size.
        /// </summary>
        /// <remarks>
        /// TODO: Really? Do we have to do this trick? It just seems... WRONG.
        /// </remarks>
        private static Tuple<int, int> _sizeCache;

        /// <summary>
        /// Return the size so the layout system can calculate the proper
        /// size for this.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        /// <remarks>
        /// The VM's LoadSize must have been called before this guy gets called, or this call will
        /// cause a crash!!
        /// </remarks>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (ViewModel != null)
            {
                _sizeCache = ViewModel.CalcRenderingSize(RespectRenderingDimension, availableSize.Width, availableSize.Height);
            }
            if (_sizeCache == null)
            {
                return base.MeasureOverride(availableSize);
            }

            return new Size(_sizeCache.Item1, _sizeCache.Item2);
        }

        /// <summary>
        /// Hold onto the view model, which we will need for doing all sorts of things.
        /// </summary>
        public PDFPageViewModel ViewModel
        {
            get { return (PDFPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PDFPageViewModel), typeof(PDFPageUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (PDFPageViewModel)value; }
        }
    }
}
