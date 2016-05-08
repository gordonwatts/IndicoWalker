using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using System.Reactive;
using System.Reactive.Subjects;

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
    public static class MRUSettingsCache
    {
        /// <summary>
        /// WHat we use to access the remote MRU lists.
        /// </summary>
        private const string RemoteMRUSettingsName = "RemoteMRULists";

        /// <summary>
        /// Cache our private version of the update stream.
        /// </summary>
        /// <remarks>
        /// It will fire if we update the local cache or if someone remote updates the
        /// local data.
        /// </remarks>
        private static Lazy<Subject<Unit>> _remoteMachineCacheUpdate = null;

        /// <summary>
        /// Get everything setup
        /// </summary>
        static MRUSettingsCache()
        {
            Init();
        }

        /// <summary>
        /// Initialize everything
        /// </summary>
        private static void Init()
        {
            _remoteMachineCacheUpdate = new Lazy<Subject<Unit>>(() =>
            {
                var r = new Subject<Unit>();

                // When it comes in from a roaming data update by the OS.
                ApplicationData.Current.DataChanged += (sender, args) => r.OnNext(default(Unit));

                return r;
            });
        }

        /// <summary>
        /// Fires when the remote machine cache is updated (e.g. a file changes, etc.)
        /// It doesn't know enough to be able to say why it updated. Just touching a file, for example,
        /// may be enough to trigger this.
        /// </summary>
        /// <remarks>
        /// It will not cause a trigger the first time you hook up. You'll have to wait until
        /// something changes before that happens
        /// </remarks>
        public static IObservable<Unit> RemoteMachineCacheUpdate { get { return _remoteMachineCacheUpdate.Value; } }

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

            // Let the rest of the application know
            _remoteMachineCacheUpdate.Value.OnNext(default(Unit));
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

            // Reset the notification fellow as well
            _remoteMachineCacheUpdate = null;
            Init();
        }
    }
}
