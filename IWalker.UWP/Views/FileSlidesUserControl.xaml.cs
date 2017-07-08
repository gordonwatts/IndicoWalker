using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    /// <summary>
    /// The view for the file slides. We are a list of all the slide thumbnails.
    /// </summary>
    public sealed partial class FileSlidesUserControl : UserControl, IViewFor<FileSlideListViewModel>
    {
        public FileSlidesUserControl()
        {
            this.InitializeComponent();

            var gc = new CompositeDisposable();
            gc.Add(this.OneWayBind(ViewModel, x => x.SlideThumbnails, y => y.Slides.ItemsSource));
            gc.Add(this.WhenAny(x => x.ViewModel, y => y.Value).Where(v => v == null).Subscribe(_ => Slides.ItemsSource = null));

            this.WhenActivated(disposeOfMe =>
            {
                if (gc != null)
                {
                    disposeOfMe(gc);
                    gc = null;
                }
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
