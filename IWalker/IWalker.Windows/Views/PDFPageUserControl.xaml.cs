using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

            // If the size alters, we need to make sure it gets updated on the VM so rendering is done "right"
            SizeChanged += (s, e) =>
            {
                if (ViewModel != null)
                {
                    ViewModel.RenderHeight = e.NewSize.Height;
                    ViewModel.RenderWidth = e.NewSize.Width;
                }
            };
        }

        /// <summary>
        /// Get/set the rendering dimension to respect.
        /// </summary>
        public PDFPageViewModel.RenderingDimension RespectRenderingDimension
        {
            get { return (PDFPageViewModel.RenderingDimension)GetValue(RespectRenderingDimensionProperty); }
            set {
                SetValue(RespectRenderingDimensionProperty, value);
                if (ViewModel != null)
                {
                    ViewModel.RenderingPriority = value;
                }
            }
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
            set { 
                SetValue(ViewModelProperty, value);
                // TODO: we shouldn't need this!!
                ViewModel.RenderingPriority = RespectRenderingDimension;
            }
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
