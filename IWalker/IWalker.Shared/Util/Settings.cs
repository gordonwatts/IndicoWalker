﻿using System;
using Windows.Storage;

namespace IWalker.Util
{
    /// <summary>
    /// Store settings...
    /// </summary>
    public class Settings
    {
        public static bool FirstTimeWeHaveRun
        {
            get
            {
                if (ApplicationData.Current.RoamingSettings.Values.Keys.Contains("HasAlreadyRun"))
                {
                    return false;
                }
                ApplicationData.Current.RoamingSettings.Values["HasAlreadyRun"] = "yes";
                return true;
            }
        }

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

        /// <summary>
        /// Get the amount of time that PDF pages are allowed to remain in the cache
        /// </summary>
        public static TimeSpan PageCacheTime
        {
            get { return TimeSpan.FromDays(3); }
        }

        /// <summary>
        /// Get/Set if meeting talks should be automatically downloaded (and thus cached) when we open a meeting.
        /// </summary>
        public static bool AutoDownloadNewMeeting
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("AutoDownloadNewMeeting"))
                {
                    return (bool)ApplicationData.Current.LocalSettings.Values["AutoDownloadNewMeeting"];
                }
#if WINDOWS_APP
                return true;
#endif
#if WINDOWS_PHONE_APP
                return false;
#endif
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["AutoDownloadNewMeeting"] = value;
            }
        }

        /// <summary>
        /// How far back should we ask for meetings?
        /// </summary>
        public static int DaysBackToFetchMeetings
        {
            get { return 60; }
        }

        /// <summary>
        /// How long before we re-fetch the meeting list when we are being "nice".
        /// Returns in hours.
        /// </summary>
        public static double MeetingCategoryStaleHours { get { return 6; } }

        /// <summary>
        /// How long from the current time should we show the a meeting on the main page?
        /// </summary>
        public static double DaysOfUpcomingMeetingsToShowOnMainPage { get { return 14; } }
    }
}
