using IWalker.Util;
using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;

// The Settings Fly-out item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace IWalker.Views
{
    public sealed partial class BasicSettingsFlyout : SettingsFlyout
    {
        private IScreen _screen;

        /// <summary>
        /// Get the item setup.
        /// </summary>
        /// <param name="screen"></param>
        public BasicSettingsFlyout(IScreen screen)
        {
            this.InitializeComponent();
            _screen = screen;

            var timeList = ExpirationOptions.GetListExpirationOptions();
            ClearCacheAgenda.ItemsSource = timeList;
            ClearCacheTalkFiles.ItemsSource = timeList;

            // Set the currently selected times
            var s = Settings.CacheAgendaTime;
            ClearCacheAgenda.SelectedItem = timeList.Where(x => x.Time == s).FirstOrDefault();
            s = Settings.CacheFilesTime;
            ClearCacheTalkFiles.SelectedItem = timeList.Where(x => x.Time == s).FirstOrDefault();

            // And the auto download
            AutoDownload.IsOn = Settings.AutoDownloadNewMeeting;
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
            Blobs.LocalStorage.InvalidateAll();
        }

        /// <summary>
        /// Change the amount of time we cache a selection for.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCacheAgenda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelection(e.AddedItems.First() as ExpirationOptions.CacheTime, x => Settings.CacheAgendaTime = x);
        }

        /// <summary>
        /// Cache the selection that has changed.
        /// </summary>
        /// <param name="cacheTime"></param>
        private void UpdateSelection(ExpirationOptions.CacheTime cacheTime, Action<TimeSpan> setTime)
        {
            setTime(cacheTime.Time);
        }

        /// <summary>
        /// When there is a change, record it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCacheTalkFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelection(e.AddedItems.First() as ExpirationOptions.CacheTime, x => Settings.CacheFilesTime = x);
        }

        /// <summary>
        /// What the user wants to do with auto download has switched.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoDownload_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Settings.AutoDownloadNewMeeting = AutoDownload.IsOn;
        }
    }
}
