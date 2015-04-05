using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class FileSlidesUserControl : UserControl, IViewFor<FileSlideListViewModel>
    {
        public FileSlidesUserControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Stash the view model
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
