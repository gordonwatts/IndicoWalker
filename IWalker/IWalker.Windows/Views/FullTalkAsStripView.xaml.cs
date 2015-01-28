using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

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
            Observable.FromEventPattern<KeyRoutedEventArgs>(this, "KeyUp")
                .Select(args => args.EventArgs)
                .Where(keys => keys.Key == VirtualKey.Escape)
                .Where(keys => ViewModel != null)
                .Do(keys => keys.Handled = true)
                .Subscribe(e => ViewModel.GoBack.Execute(null));            
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
