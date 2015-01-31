using IWalker.Views;
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
        /// IsInViewport - true for objects that are visible in the view port.
        /// </summary>
        public static readonly DependencyProperty IsInViewportProperty =
            DependencyProperty.RegisterAttached("IsInViewport", typeof(bool), typeof(OnScreenTrackingHelper), new PropertyMetadata(false));

        /// <summary>
        /// Get the InViewport for a particular UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool GetIsInViewport(UIElement element)
        {
            return (bool)element.GetValue(IsInViewportProperty);
        }

        /// <summary>
        /// Set the InViewport for a particular UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetIsInViewport(UIElement element, bool value)
        {
            Debug.WriteLine("Setting IsInViewport to {0} on {1} for hash {2}", value, element.GetType().Name, element.GetHashCode());
            element.SetValue(IsInViewportProperty, value);
        }

        /// <summary>
        /// Attach ourselves to a scroll viewer.
        /// </summary>
        /// <param name="host"></param>
        public OnScreenTrackingHelper(ScrollViewer host)
        {
            Debug.Assert(host != null);

            _host = host;
            _host.ViewChanged += host_ViewChanged;
        }

        /// <summary>
        /// Fired when the view has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void host_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Do nothing if this is intermediate.
            if (e.IsIntermediate)
                return;

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
                if ((frameInfo.Item1 as PDFPageUserControl) != null)
                {
                    (frameInfo.Item1 as PDFPageUserControl).ShowPDF = frameInfo.Item2;
                }
                //SetIsInViewport(frameInfo.Item1, frameInfo.Item2);
            }
        }

        /// <summary>
        /// See if any portion of the given frame is on screen.
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
#if false
    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        base.OnScrollChanged(e);

        var panel = Content as Panel;
        if (panel == null)
        {
            return;
        }

        Rect viewport = new Rect(new Point(0, 0), RenderSize);

        foreach (UIElement child in panel.Children)
        {
            if (!child.IsVisible)
            {
                SetIsInViewport(child, false);
                continue;
            }

            GeneralTransform transform = child.TransformToAncestor(this);
            Rect childBounds = transform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
            SetIsInViewport(child, viewport.IntersectsWith(childBounds));
        }
    }
#endif
#if false
        <Window x:Class="..."
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:my="clr-namespace:...">

    <Window.Resources>
        <DataTemplate DataType="{x:Type my:MovieViewModel}">
            <StackPanel>
                <Image x:Name="Thumbnail" Stretch="Fill" Width="100" Height="100" />
                <TextBlock Text="{Binding Name}" />
            </StackPanel>
            <DataTemplate.Triggers>
                <DataTrigger Binding="{Binding Path=(my:MyScrollViewer.IsInViewport), RelativeSource={RelativeSource AncestorType={x:Type ListBoxItem}}}"
                             Value="True">
                    <Setter TargetName="Thumbnail" Property="Source" Value="{Binding Thumbnail}" />
                </DataTrigger>
            </DataTemplate.Triggers>
        </DataTemplate>
    </Window.Resources>

    <ListBox ItemsSource="{Binding Movies}">
        <ListBox.Template>
            <ControlTemplate>
                <my:MyScrollViewer HorizontalScrollBarVisibility="Disabled" VerticalScrollBarVisibility="Auto">
                    <WrapPanel IsItemsHost="True" />
                </my:MyScrollViewer>
            </ControlTemplate>
        </ListBox.Template>
    </ListBox>

</Window>
#endif
    }
}