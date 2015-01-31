using System;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace IWalker.Utilities
{
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
        /// Attach ourselves to a scroll viewer.
        /// </summary>
        /// <param name="hostScrollViewer">The ScrollViewer that we will send updates for</param>
        /// <param name="updateInViewPort">Method will be called to turn on/off each UIElement that is part of each ContentPresenter that the ScrollViewer is holding in its ItemsControl.</param>
        public OnScreenTrackingHelper(ScrollViewer hostScrollViewer, Action<UIElement, bool> updateInViewPort)
        {
            Debug.Assert(hostScrollViewer != null);

            _host = hostScrollViewer;
            _updateFunction = updateInViewPort;

            _host.ViewChanged += host_ViewChanged;
        }

        /// <summary>
        /// Fired when the view has changed. Update who is in and who isn't in the ViewPort.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void host_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
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