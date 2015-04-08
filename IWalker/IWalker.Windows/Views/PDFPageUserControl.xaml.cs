using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
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
        public PDFPageUserControl()
        {
            this.InitializeComponent();

            this.WhenActivated(disposeOfMe =>
            {
                // The image source
                disposeOfMe(this.WhenAny(x => x.ViewModel, x => x.Value)
                    .Where(vm => vm != null)
                    .Subscribe(vm => vm.ImageStream
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .SelectMany(async imageStream =>
                        {
                            imageStream.Seek(0, SeekOrigin.Begin);
                            var bm = new BitmapImage();
                            await bm.SetSourceAsync(WindowsRuntimeStreamExtensions.AsRandomAccessStream(imageStream));
                            imageStream.Dispose();
                            return bm;
                        })
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(image => ThumbImage.Source = image)));

                // Now, when something about our size and rendering stuff changes, we need
                // to shoot off a rendering request. Don't do it if we have already requested it, however!
                disposeOfMe(this.Events().SizeChanged.Select(a => default(Unit))
                    .Merge(this.WhenAny(x => x.ShowPDF, x => default(Unit)))
                    .Buffer(TimeSpan.FromMilliseconds(250)).Where(l => l != null && l.Count > 0).Select(l => default(Unit))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Where(_ => ShowPDF)
                    .Where(_ => ViewModel != null)
                    .Select(_ => RespectRenderingDimension)
                    .Select(t => Tuple.Create(t, ActualWidth, ActualHeight))
                    .DistinctUntilChanged()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(t => ViewModel.RenderImage.Execute(t)));
            });
        }

        /// <summary>
        /// Depending on what mode we are operating in, determine the size, and let the layout system know what we want to be.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (ViewModel != null)
            {
                var requestedSize = ViewModel.CalcRenderingSize(RespectRenderingDimension, availableSize.Width, availableSize.Height);
                return new Size(requestedSize.Item1, requestedSize.Item2);
            }
            else
            {
                return base.MeasureOverride(availableSize);
            }
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
