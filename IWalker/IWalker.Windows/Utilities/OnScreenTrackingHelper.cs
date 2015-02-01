using System;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace IWalker.Utilities
{
    /// <summary>
    /// Helper that attaches to a ScrollViewer and sends messages to the elements in its ItemsControl indicating if their
    /// are in or out side of the ViewPort.
    /// </summary>
    /// <remarks>
    /// Use virtualization (like VirtualizingStackPanel) if you want the controls not to be created. Use this if you have
    /// a small number of controls that hold onto very large resources when they are on screen. You can use this to tell
    /// them to release the resources when off screen, and gather them up again when they are on screen.
    /// </remarks>
    public sealed class OnScreenTrackingHelper
    {
        /// <summary>
        /// Hold onto the host control.
        /// </summary>
        private ScrollViewer _host;

        /// <summary>
        /// Hold onto the action we will perform on each UIElement.
        /// </summary>
        private Action<UIElement, bool> _updateFunction;

        /// <summary>
        /// Get set the number of items on either side of the visible region to set as in view port.
        /// This makes the scrolling experience slightly more pleasent. Size will depend on
        /// a lot of application specific things. Defaults to zero.
        /// </summary>
        /// <remarks>
        /// Updating this won't cause an update immediately to the state of each item. It will be factored
        /// in upon the next normally occuring update.
        /// </remarks>
        public int ItemsWaitingInTheWings { get; set; }

        /// <summary>
        /// Attach ourselves to a scroll viewer.
        /// </summary>
        /// <param name="hostScrollViewer">The ScrollViewer that we will send updates for</param>
        /// <param name="updateInViewPort">Method will be called to turn on/off each UIElement that is part of each ContentPresenter that the ScrollViewer is holding in its ItemsControl.</param>
        public OnScreenTrackingHelper(ScrollViewer hostScrollViewer, Action<UIElement, bool> updateInViewPort)
        {
            Debug.Assert(hostScrollViewer != null);

            _host = hostScrollViewer;
            _updateFunction = updateInViewPort;

            // By default do no buffer renderings.
            ItemsWaitingInTheWings = 0;

            // When the scroller is loaded and when people scroll, we
            // must update who is in and who is out.
            _host.ViewChanged += (s, a) => { if (a.IsIntermediate) UpdateWhoIsInViewPort(); };
            _host.Loaded += (s, a) => UpdateWhoIsInViewPort();
            _host.LayoutUpdated += (s, a) => UpdateWhoIsInViewPort();
        }

        /// <summary>
        /// Fired when the view has changed. Update who is in and who isn't in the ViewPort.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateWhoIsInViewPort()
        {
            // What is the view port that is visible on the screen?
            var viewport = new Rect(new Point(0, 0), _host.RenderSize);

            // Go through everything owned by the scroll bar's panel, and set it.
            // Normally a content presenter holds just one item, but a DataTemplate (or other) could have multiple,
            // so we will be sure to support all children of the content presenter.
            var itemContainer = (_host.Content as ItemsControl);
            var inframe = from index in Enumerable.Range(0, itemContainer.Items.Count)
                          let container = itemContainer.ContainerFromIndex(index) as ContentPresenter
                          let inFrame = isInFrame(viewport, container)
                          select Tuple.Create(container, inFrame);

            // We want to turn on a few for a buffer as well.
            var initialList = inframe.TakeWhile(t => t.Item2 == false);
            var inViewPortList = inframe.Where(t => t.Item2);
            var tailList = inframe.SkipWhile(t => t.Item2 == false).Where(t => t.Item2 == false);

            var nbuf = initialList.Count() - ItemsWaitingInTheWings;
            if (nbuf > 0)
            {
                initialList = initialList.Take(nbuf)
                    .Concat(initialList.Skip(nbuf).Select(t => Tuple.Create(t.Item1, true)));
            }

            tailList =
                tailList.Take(ItemsWaitingInTheWings).Select(t => Tuple.Create(t.Item1, true))
                .Concat(tailList.Skip(ItemsWaitingInTheWings));

            inframe = initialList.Concat(inViewPortList).Concat(tailList);

            // Ok - and finally issue a "set" for all the visual elements in the scrolling view.
            var allToSet = from p in inframe
                           let cnt = VisualTreeHelper.GetChildrenCount(p.Item1)
                           from childIndex in Enumerable.Range(0, cnt)
                           let child = VisualTreeHelper.GetChild(p.Item1, childIndex) as UIElement
                           where (child != null)
                           select Tuple.Create(child, p.Item2);

            foreach (var frameInfo in allToSet)
            {
                _updateFunction(frameInfo.Item1, frameInfo.Item2);
            }
        }

        /// <summary>
        /// Helper function to see if any portion of the given frame is on screen.
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool isInFrame(Rect viewport, ContentPresenter content)
        {
            var transform = content.TransformToVisual(_host);
            var childBounds = transform.TransformBounds(new Rect(new Point(0, 0), content.RenderSize));
            childBounds.Intersect(viewport);
            return !childBounds.IsEmpty;
        }
    }
}