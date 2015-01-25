using Windows.Storage;

namespace IWalker.Util
{
    /// <summary>
    /// Store settings...
    /// </summary>
    class Settings
    {
        public static string LastViewedMeeting
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("LastViewedMeeting"))
                {
                    return ApplicationData.Current.LocalSettings.Values["LastViewedMeeting"] as string;
                }
                return "";
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["LastViewedMeeting"] = value;
            }
        }
    }
}
