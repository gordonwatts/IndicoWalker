using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class FileView : UserControl, IViewFor<FileUserControlViewModel>
    {
        public FileView()
        {
            this.InitializeComponent();
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.BindCommand(ViewModel, x => x.ClickedUs, y => y.FileClick));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.FileNotCached, y => y.DownloadIcon.Visibility));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.IsDownloading, y => y.DownloadProgress.IsActive));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.DocumentTypeString, y => y.DocumentType.Text));
            });
        }

        public FileUserControlViewModel ViewModel
        {
            get { return (FileUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(FileUserControlViewModel), typeof(FileView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (FileUserControlViewModel)value; }
        }
    }
}
