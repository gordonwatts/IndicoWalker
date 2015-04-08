using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    /// <summary>
    /// The view for the file slides. We are mostly a list of all the slide thumbnails.
    /// </summary>
    public sealed partial class FileSlidesUserControl : UserControl, IViewFor<FileSlideListViewModel>
    {
        public FileSlidesUserControl()
        {
            this.InitializeComponent();
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.SlideThumbnails, y => y.Slides.ItemsSource));
            });
        }

        /// <summary>
        /// Hold onto the view model, which we will need for doing all sorts of things.
        /// </summary>
        public FileSlideListViewModel ViewModel
        {
            get { return (FileSlideListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(FileSlideListViewModel), typeof(FileSlidesUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (FileSlideListViewModel)value; }
        }
    }
}
