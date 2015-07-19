using IWalker.Util;
using IWalker.ViewModels;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IWalker.Views
{
    /// <summary>
    /// View for a PDF talk as a full list of slides (full screen) with keys and other things to navagate around.
    /// </summary>
    public sealed partial class FullTalkAsStripView : Page, IViewFor<FullTalkAsStripViewModel>
    {
        public FullTalkAsStripView()
        {
            this.InitializeComponent();
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.Pages, y => y.SlideStrip.ItemsSource));

                // If the ESC key or backbutton is hit, we want to navigate back.
                var keyrelease = SlideStrip.Events().KeyDown
                    .Where(keys => ViewModel != null);

                keyrelease
                    .Where(keys => keys.Key == VirtualKey.Escape)
                    .Do(keys => keys.Handled = true)
                    .Subscribe(e => Locator.Current.GetService<RoutingState>().NavigateBack.Execute(null));

                backButton.WireAsBackButton();

                // Forward and backwards arrows.
                // Tricky because if we calcCurrentPage while in the middle of the scroll we won't
                // get a scroll to the item we want. So we need to aggregate those while running.
                var isScrollInProgress = theScrollViewer
                    .Events().ViewChanged
                    .Select(sc => sc.IsIntermediate);

                var keysByScrolling = keyrelease
                        .Select(k => Tuple.Create(k, calcKeyMoveRequest(k)))
                        .Where(k => k.Item2 != 0)
                        .Do(k => k.Item1.Handled = true)
                        .Select(k => k.Item2)
                        .BatchByStream(isScrollInProgress);

                keysByScrolling
                    .Where(info => !info.Item1)
                    .Subscribe(info => info.Item2.Subscribe(delta => ViewModel.PageMove.Execute(calcCurrentPage() + delta)));

                keysByScrolling
                    .Where(info => info.Item1)
                    .SelectMany(async info => await info.Item2.Sum())
                    .Where(d => d != 0)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(delta => ViewModel.PageMove.Execute(delta + calcCurrentPage()));

                // We can't tell what size things are in here (which we need for scrolling, etc.) until
                // we have a clue as to what the layout is. So, we have to wait for that to go.
                var widthOfItemsChanged = SlideStrip.Events().SizeChanged
                    .Select(_ => SlideStrip.ActualHeight)
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .DistinctUntilChanged();

                disposeOfMe(widthOfItemsChanged
                    .Subscribe(_ => _slideStartLocations = null));

                // And when we get asked to bring a page into view...
                disposeOfMe(this.WhenAny(x => x.ViewModel, x => x.Value)
                    .Where(x => x != null)
                    .Subscribe(vm =>
                    {
                        vm.MoveToPage
                            .CombineLatest(widthOfItemsChanged, (pn, width) => pn)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Where(pn => SlideStrip.ContainerFromIndex(0) != null)
                            .Select(pn => getSlideEdge(pn))
                            .Subscribe(loc =>
                            {
                                if (_orientation == FullPanelOrientation.Horizontal)
                                {
                                    theScrollViewer.ChangeView(loc, null, null);
                                }
                                else
                                {
                                    theScrollViewer.ChangeView(null, loc, null);
                                }
                            });
                    }));

                // Make the back button visible if there is any movement - scrolling or otherwise.
                var buttonRelatedMovement =
                    Observable.Merge(
                        this.Events().PointerMoved.Select(x => Unit.Default),
                        this.Events().Tapped.Select(x => Unit.Default)
                    );

                var makeVisible = buttonRelatedMovement
                    .Select(x => true);

                var makeInvisible = buttonRelatedMovement
                    .Select(x => Observable.Return(false).Delay(TimeSpan.FromSeconds(3)))
                    .Switch();

                Observable.Merge(makeVisible, makeInvisible)
                    .DistinctUntilChanged()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(v => backButton.Visibility = v ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed);
                backButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                // Setup the rendering helper - when they are in frame, cause a rendering to happen. When they
                // are out of frame, then turn off showing everything!
                _holder = new OnScreenTrackingHelper(
                    theScrollViewer,
                    (uiElement, inViewPort) =>
                    {
                        if (uiElement is PDFPageUserControl)
                        {
                            (uiElement as PDFPageUserControl).ShowPDF = inViewPort;
                        }
                    }
                    ) { ItemsWaitingInTheWings = 2 };

                this.Events().Unloaded
                    .Subscribe(t => _holder.Unload());

                // We want to capture key strokes, etc. By default we don't have
                // the focus, so grab it.
                Focus(Windows.UI.Xaml.FocusState.Programmatic);

                // The orientation of this pannel will affect how we calc the arrow key stuff.
                _orientation = theScrollViewer.VerticalScrollMode == ScrollMode.Disabled ? FullPanelOrientation.Horizontal : FullPanelOrientation.Vertical;
            });
        }

        /// <summary>
        /// Turn the keystrokes into a page movement
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        private int calcKeyMoveRequest(Windows.UI.Xaml.Input.KeyRoutedEventArgs keys)
        {
            if (keys.Key == VirtualKey.Right || keys.Key == VirtualKey.Down || keys.Key == VirtualKey.Space)
                return +1;
            if (keys.Key == VirtualKey.Left || keys.Key == VirtualKey.Up)
                return -1;
            return 0;
        }

        /// <summary>
        /// Keep track of how we are going to do the scrolling.
        /// </summary>
        private FullPanelOrientation _orientation;

        /// <summary>
        /// Hold onto the scroll helper.
        /// </summary>
        private OnScreenTrackingHelper _holder;

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
            var leftEdge = _orientation == FullPanelOrientation.Horizontal ? theScrollViewer.HorizontalOffset : theScrollViewer.VerticalOffset;
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
                    .Select(container => _orientation == FullPanelOrientation.Horizontal ? container.ActualWidth : container.ActualHeight)
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
            return _slideStartLocations[slide - 1];
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

        /// <summary>
        /// What orientation is this pannel?
        /// </summary>
        public enum FullPanelOrientation
        {
            Horizontal, Vertical
        }
    }
}

