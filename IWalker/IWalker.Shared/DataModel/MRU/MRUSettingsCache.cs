using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace IWalker.DataModel.MRU
{
    /// <summary>
    /// Store the last 20 meetings we've visited in a cache.
    /// That way others can look and use it to keep them approximately
    /// up to date.
    /// </summary>
    /// <remarks>
    /// Eventually this should be replaced by a real system that will store everything in the cloud.
    /// </remarks>
    public class MRUSettingsCache
    {
        /// <summary>
        /// WHat we use to access the remote MRU lists.
        /// </summary>
        private const string RemoteMRUSettingsName = "RemoteMRULists";

        /// <summary>
        /// Grab the MRU lists from all other machines, combine them, and return them.
        /// It does make sure that all returned lists contain no duplicates.
        /// </summary>
        /// <returns></returns>
        public static IWalker.MRU[] GetAllMachineMRUMeetings()
        {
            var settings = GetOrCreateSettingsContainer();
            var allMeetings = settings.Values.Keys
                .Select(k => settings.Values[k] as string)
                .SelectMany(json => JsonConvert.DeserializeObject<IWalker.MRU[]>(json));

            var latestMeetings = allMeetings
                .GroupBy(m => m.IDRef)
                .Select(grp => grp.OrderByDescending(m => m.LastLookedAt).First());

            return latestMeetings.ToArray();
        }

        /// <summary>
        /// Get the data from a single machine
        /// </summary>
        /// <param name="machineName">Name of the machine from which to fetch the data</param>
        /// <returns>The list of MRU's for the given machine or null</returns>
        public static IWalker.MRU[] GetFromMachine(string machineName)
        {
            var settings = GetOrCreateSettingsContainer();
            if (!settings.Values.ContainsKey(machineName))
                return null;
            var json = settings.Values[machineName] as string;
            return JsonConvert.DeserializeObject<IWalker.MRU[]>(json);
        }

        /// <summary>
        /// Save everything for a particular machine
        /// </summary>
        /// <param name="machineName">Name of the machine we should store or update this list</param>
        /// <param name="mrus">List of MRU's to store</param>
        public static void UpdateForMachine(string machineName, IWalker.MRU[] mrus)
        {
            var settings = GetOrCreateSettingsContainer();
            settings.Values[machineName] = JsonConvert.SerializeObject(mrus);
        }

        /// <summary>
        /// Returns the MRU settings container
        /// </summary>
        /// <returns></returns>
        private static ApplicationDataContainer GetOrCreateSettingsContainer()
        {
            ApplicationData.Current.RoamingSettings.CreateContainer(RemoteMRUSettingsName, ApplicationDataCreateDisposition.Always);
            return ApplicationData.Current.RoamingSettings.Containers[RemoteMRUSettingsName];
        }

        /// <summary>
        /// Delete everything in the MRU cache - this is to help with testing.
        /// </summary>
        public static void ResetCache()
        {
            if (ApplicationData.Current.RoamingSettings.Containers.ContainsKey(RemoteMRUSettingsName))
            {
                ApplicationData.Current.RoamingSettings.DeleteContainer(RemoteMRUSettingsName);
            }
        }
    }
}
