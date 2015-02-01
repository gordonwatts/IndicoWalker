using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

            // The image source
            this.OneWayBind(ViewModel, x => x.Image, y => y.ThumbImage.Source);

            // Now, when something about our size and rendering stuff changes, we need
            // to shoot off a rendering request.
            this.Events().SizeChanged.Select(a => RespectRenderingDimension)
                .Merge(this.WhenAny(x => x.ShowPDF, x => RespectRenderingDimension))
                .Delay(TimeSpan.FromSeconds(1)).ObserveOn(RxApp.MainThreadScheduler)
                .Where(x => ShowPDF)
                .Where(t => ViewModel != null)
                .Subscribe(t => ViewModel.RenderImage.Execute(Tuple.Create(t, ActualWidth, ActualHeight)));

            this.WhenAny(x => x.ShowPDF, x => x.Value)
                .Where(x => ViewModel != null)
                .Subscribe(x => ViewModel.AttachImage = x);

            //var benow = this.GetBindingExpression(ShowPDFProperty);
            //this.Events().Loaded
            //    .Delay(TimeSpan.FromSeconds(10))
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Do(x => benow = this.GetBindingExpression(ShowPDFProperty))
            //    .Subscribe(a => OnScreenTrackingHelper.SetIsInViewport(this, true));

            //this.WhenAny(x => x.ShowPDF, x => x.Value)
            //    .Do(x => benow = this.GetBindingExpression(ShowPDFProperty))
            //    .Subscribe(v => Debug.WriteLine("ShowPDF updated to {0} on {1}", v, GetHashCode()));

        }

        /// <summary>
        /// Depending on what mode we are operating in, determine the size, and let the layout system know what we want to be.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Debug.Assert(ViewModel != null);
            var requestedSize = ViewModel.CalcRenderingSize(RespectRenderingDimension, availableSize.Width, availableSize.Height);
            return new Size(requestedSize.Item1, requestedSize.Item2);
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
