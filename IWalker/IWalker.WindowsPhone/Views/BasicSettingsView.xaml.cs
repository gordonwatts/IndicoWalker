using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BasicSettingsView : Page, IViewFor<BasicSettingsViewModel>
    {
        public BasicSettingsView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public BasicSettingsViewModel ViewModel
        {
            get { return (BasicSettingsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(BasicSettingsViewModel), typeof(BasicSettingsView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (BasicSettingsViewModel)value; }
        }
    }
}
