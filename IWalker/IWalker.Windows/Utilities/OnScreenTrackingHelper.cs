using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IWalker.Utilities
{
    public sealed class OnScreenTrackingHelper
    {
        /// <summary>
        /// IsInViewport - true for objects that are visible in the view port.
        /// </summary>
        public static readonly DependencyProperty IsInViewportProperty =
            DependencyProperty.RegisterAttached("IsInViewport", typeof(bool), typeof(OnScreenTrackingHelper), new PropertyMetadata(false));
        private ScrollViewer _host;

        public static bool GetIsInViewport(UIElement element)
        {
            return (bool)element.GetValue(IsInViewportProperty);
        }

        public static void SetIsInViewport(UIElement element, bool value)
        {
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

            // Go through everything owned by the scroll bar's panel.

            var itemContainer = (_host.Content as ItemsControl);
            var inframe = from index in Enumerable.Range(0, itemContainer.Items.Count)
                          let container = itemContainer.ContainerFromIndex(index) as ContentPresenter
                          let item = itemContainer.ItemFromContainer(container)
                          let inFrame = isInFrame(viewport, container)
                          select Tuple.Create(container, inFrame);
                //.Select(index => itemContainer.ContainerFromIndex(index) as ContentPresenter)
                //.Select(content => Tuple.Create(content, isInFrame(viewport, content)));

            foreach (var frameInfo in inframe)
            {
                SetIsInViewport(frameInfo.Item1, frameInfo.Item2);
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