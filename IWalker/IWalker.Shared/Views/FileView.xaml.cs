using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

            var gc = new CompositeDisposable();

            gc.Add(this.BindCommand(ViewModel, x => x.ClickedUs, y => y.FileClick));
            gc.Add(this.OneWayBind(ViewModel, x => x.FileNotCachedOrDownloading, y => y.DownloadIcon.Visibility));
            gc.Add(this.OneWayBind(ViewModel, x => x.IsDownloading, y => y.DownloadProgress.IsActive));
            gc.Add(this.OneWayBind(ViewModel, x => x.DocumentTypeString, y => y.DocumentType.Text));

            gc.Add(this.WhenAny(x => x.ViewModel, x => x.Value)
                .Where(vm => vm != null)
                .Subscribe(vm => vm.OnLoaded.Execute(null)));

            this.WhenActivated(disposeOfMe =>
            {
                if (gc != null)
                {
                    disposeOfMe(gc);
                    gc = null;
                }
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
