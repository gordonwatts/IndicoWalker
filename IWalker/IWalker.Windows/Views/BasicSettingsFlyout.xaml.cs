using Akavache;
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

        /// <summary>
        /// Delete everything we know about the local cache when they want to delete it!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCache_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            BlobCache.UserAccount.InvalidateAll();
        }
    }
}
