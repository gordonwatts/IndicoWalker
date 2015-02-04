using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace IWalker.Views
{
    public sealed partial class BasicSettingsFlyout : SettingsFlyout
    {
        private IScreen _screen;

        public BasicSettingsFlyout(IScreen screen)
        {
            this.InitializeComponent();
            _screen = screen;
        }

        /// <summary>
        /// Go to the normal certificate settings page when we need to do some sort of update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenSecurityPage_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _screen.Router.Navigate.Execute(new BasicSettingsViewModel(_screen));
        }
    }
}
