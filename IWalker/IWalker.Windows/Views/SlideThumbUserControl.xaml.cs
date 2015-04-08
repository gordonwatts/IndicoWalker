using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    /// <summary>
    /// Show the image. When we are re-sized, we will cause a rendering for our image.
    /// </summary>
    public sealed partial class SlideThumbUserControl : UserControl, IViewFor<SlideThumbViewModel>
    {
        public SlideThumbUserControl()
        {
            this.InitializeComponent();
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.PDFPageVM, y => y.PDFPageUC.ViewModel));

                // If they mousedown or tap and release in this image, then we want to open
                // the full screen display starting from this image.
                var pressed = PDFPageUC.Events().PointerPressed;
                var released = PDFPageUC.Events().PointerReleased;
                var when = from pd in pressed
                           from pu in released
                           select Unit.Default;
                disposeOfMe(when.Subscribe(e => ViewModel.OpenFullView.Execute(null)));
            });

        }

        /// <summary>
        /// Hold onto the view model, which we will need for doing all sorts of things.
        /// </summary>
        public SlideThumbViewModel ViewModel
        {
            get { return (SlideThumbViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SlideThumbViewModel), typeof(SlideThumbUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (SlideThumbViewModel)value; }
        }
    }
}
