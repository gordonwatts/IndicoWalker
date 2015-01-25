using IWalker.ViewModels;
using ReactiveUI;
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
            this.OneWayBind(ViewModel, x => x.Image, y => y.ThumbImage.Source);
            this.Bind(ViewModel, x => x.RenderWidth, y => y.ThumbImage.Width);
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
