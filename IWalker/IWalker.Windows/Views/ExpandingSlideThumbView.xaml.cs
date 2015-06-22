using IWalker.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class ExpandingSlideThumbView : UserControl, IViewFor<ExpandingSlideThumbViewModel>
    {
        public ExpandingSlideThumbView()
        {
            this.InitializeComponent();

            var gc = new CompositeDisposable();
            gc.Add(this.OneWayBind(ViewModel, x => x.TalkAsThumbs, y => y.SlidesAsThumbs.ViewModel));
            gc.Add(this.BindCommand(ViewModel, x => x.ShowSlides, y => y.ShowThumbs));
            gc.Add(this.OneWayBind(ViewModel, x => x.NumberOfSlides, y => y.ShowThumbs.Content, np => string.Format("({0} thumbnails)", np)));
            gc.Add(this.OneWayBind(ViewModel, x => x.CanShowThumbs, y => y.ShowThumbs.Visibility));

            // Wire it up!

            // Remove all subscriptions after we have been shown.
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
        public ExpandingSlideThumbViewModel ViewModel
        {
            get { return (ExpandingSlideThumbViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ExpandingSlideThumbViewModel), typeof(ExpandingSlideThumbView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ExpandingSlideThumbViewModel)value; }
        }
    }
}
