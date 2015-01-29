using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FullTalkAsStripView : Page, IViewFor<FullTalkAsStripViewModel>
    {
        public FullTalkAsStripView()
        {
            this.InitializeComponent();
            this.OneWayBind(ViewModel, x => x.Pages, y => y.SlideStrip.ItemsSource);

            // If the ESC key is hit, we want to navigate back.
            var keyrelease = Observable.FromEventPattern<KeyRoutedEventArgs>(this.SlideStrip, "KeyDown")
                .Select(args => args.EventArgs)
                .Where(keys => ViewModel != null);

            keyrelease
                .Where(keys => keys.Key == VirtualKey.Escape)
                .Do(keys => keys.Handled = true)
                .Subscribe(e => ViewModel.GoBack.Execute(null));

            // Forward and backwards arrows
            keyrelease
                .Where(keys => keys.Key == VirtualKey.Right)
                .Do(keys => keys.Handled = true)
                .Subscribe(e => ViewModel.PageForward.Execute(calcCurrentPage()));
            keyrelease
                .Where(keys => keys.Key == VirtualKey.Left)
                .Do(keys => keys.Handled = true)
                .Subscribe(e => ViewModel.PageBack.Execute(calcCurrentPage()));

            // We can't tell what size things are in here (which we need for scrolling, etc.) until
            // we have a clue as to what the layout is. So, we have to wait for that to go.
            var widthOfItemsChanged = Observable.FromEventPattern(this.SlideStrip, "LayoutUpdated")
                .Where(args => _slideStartLocations == null)
                .Select(args => SlideStrip.ActualWidth)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Select(args => Unit.Default)
                .DistinctUntilChanged();

            // And when we get asked to bring a page into view...
            this.WhenAny(x => x.ViewModel, x => x.Value)
                .Where(x => x != null)
                .Subscribe(vm => {
                    vm.MoveToPage
                        .CombineLatest(widthOfItemsChanged, (pn, width) => pn)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(pn => SlideStrip.ContainerFromIndex(0) != null)
                        .Select(pn => getSlideEdge(pn))
                        .Subscribe(loc => theScrollViewer.ChangeView(loc, null, null));
                });
        }

        /// <summary>
        /// Keep a cache of where all the slides are so we can do this "fast"
        /// </summary>
        private double[] _slideStartLocations = null;

        /// <summary>
        /// Determine the current slide that is on screen.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is a bit tricky. We will use the right edge of the slide to figure out where we are, relative to the left edge
        /// of the scroll bar. We want the next slide *after* the first right edge.
        /// </remarks>
        private int calcCurrentPage()
        {
            calcSlideEdges();

            // Get the current view and find where it is located. The edge cases are a little tricky!
            var leftEdge = theScrollViewer.HorizontalOffset;
            var slide = Enumerable.Range(0, _slideStartLocations.Length)
                .Where(index => _slideStartLocations[index] > leftEdge)
                .FirstOrDefault();
            if (slide == 0)
            {
                return _slideStartLocations[0] >= leftEdge ? 0 : _slideStartLocations.Length - 1;
            }
            return slide;
        }

        /// <summary>
        /// Setup the slide edges
        /// </summary>
        private void calcSlideEdges()
        {
            if (_slideStartLocations == null)
            {
                double sum = 0;
                var widths = Enumerable.Range(0, SlideStrip.Items.Count)
                    .Select(index => SlideStrip.ContainerFromIndex(index) as ContentPresenter)
                    .Select(container => container.ActualWidth)
                    .Select(w => sum += w);
                _slideStartLocations = widths
                    .ToArray();
            }
        }

        /// <summary>
        /// Return the slide location.
        /// </summary>
        /// <param name="slide"></param>
        /// <returns></returns>
        private double getSlideEdge(int slide)
        {
            if (slide == 0)
                return 0;

            calcSlideEdges();
            return _slideStartLocations[slide-1];
        }

        /// <summary>
        /// Hold onto the view model, which we will need for doing all sorts of things.
        /// </summary>
        public FullTalkAsStripViewModel ViewModel
        {
            get { return (FullTalkAsStripViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(FullTalkAsStripViewModel), typeof(FullTalkAsStripView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (FullTalkAsStripViewModel)value; }
        }
    }
}
