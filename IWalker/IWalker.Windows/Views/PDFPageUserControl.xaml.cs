using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IWalker.Views
{
    /// <summary>
    /// The view for a PDF page. We are a very simple image, with a bunch of wiring behind it.
    /// </summary>
    public sealed partial class PDFPageUserControl : UserControl, IViewFor<PDFPageViewModel>
    {
        public PDFPageUserControl()
        {
            this.InitializeComponent();

            // The image source
            this.OneWayBind(ViewModel, x => x.Image, y => y.ThumbImage.Source);

            // Now, when something about our size and rendering stuff changes, we need
            // to shoot off a rendering request.
            this.WhenAny(x => x.RespectRenderingDimension, x => x.Width, x => x.Height, (r, x, y) => Tuple.Create(r.Value, x.Value, y.Value))
                .Where(t => ViewModel != null)
                .Subscribe(t => ViewModel.RenderImage.Execute(t));

            // As soon as a VM is valid, subscribe to it so we can update our own image size
            this.WhenAny(x => x.ViewModel, x => x.Value)
                .Where(vm => vm != null)
                .Subscribe(newvm =>
                {

                    newvm.UpdateImageSize
                    .Subscribe(sz =>
                    {
                        Width = sz.Item1;
                        Height = sz.Item2;
                    });
                    ViewModel.RenderImage.Execute(Tuple.Create(RespectRenderingDimension, Width, Height));
                });
        }

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
