using IWalker.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstRunView : Page, IViewFor<FirstRunViewModel>
    {
        public FirstRunView()
        {
            // Get the XAML booted up.
            this.InitializeComponent();

            // Now connect up the buttons!

            var gc = new CompositeDisposable();

            gc.Add(this.BindCommand(ViewModel, x => x.SkipDefaultCategories, y => y.Skip));
            gc.Add(this.BindCommand(ViewModel, x => x.AddDefaultCategories, y => y.Add));

            gc.Add(this.OneWayBind(ViewModel, x => x.ItemBeingFetched, y => y.LoadingWhat.Text));
            gc.Add(this.OneWayBind(ViewModel, x => x.FetchingItems, y => y.LoadingActive.IsActive));

            // Upon activation, set everything up for disposing...
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
        public FirstRunViewModel ViewModel
        {
            get { return (FirstRunViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(FirstRunViewModel), typeof(FirstRunView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (FirstRunViewModel)value; }
        }
    }
}
