﻿using System;
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

        /// <summary>
        /// Get or set the amount of time agenda data should be cached locally
        /// </summary>
        public static TimeSpan CacheAgendaTime
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("CacheAgendaTime"))
                {
                    return (TimeSpan) ApplicationData.Current.LocalSettings.Values["CacheAgendaTime"];
                }
                return TimeSpan.FromDays(365);
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["CacheAgendaTime"] = value;
            }
        }

        /// <summary>
        /// Get set the amount of time a talk file should be cached locally
        /// </summary>
        public static TimeSpan CacheFilesTime
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("CacheFilesTime"))
                {
                    return (TimeSpan)ApplicationData.Current.LocalSettings.Values["CacheFilesTime"];
                }
                return TimeSpan.FromDays(7);
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["CacheFilesTime"] = value;
            }
        }
    }
}
