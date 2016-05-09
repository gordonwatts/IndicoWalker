using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive;
using Windows.Networking.Connectivity;

namespace IWalker.DataModel.MRU
{
    /// <summary>
    /// Return a list of MRU arrays that will fire every time
    /// the MRU list changes, no matter the source:
    /// 1. We visit a new meeting locally
    /// 2. Another machine has its MRU list updated
    /// 
    /// It will always return the 20 most recently visited items, in start time order.
    /// (this it is likely that larger numbers of meetings are stored in all the lists).
    /// 
    /// This also sets up the recording so that when we do visit a new meeting we will make sure
    /// to record it in this machine's update file (which will then incorporate everything and
    /// be circulated to all other machines that are running this).
    /// </summary>
    public static class MRUListUpdateStream
    {
        /// <summary>
        /// Configure the number of meetings in the MRU list.
        /// </summary>
        const int NumberOfMRUMeetings = 20;

        /// <summary>
        /// Cache the Rx stream that generates the MRU list for the outside world
        /// </summary>
        static IObservable<IWalker.MRU[]> _MRUStream = null;

        /// <summary>
        /// Used only for testing!
        /// </summary>
        public static void Reset()
        {
            _MRUStream = null;
            _MRUDBSubscription = null;
            _machineName = null;
        }

        /// <summary>
        /// Get/Set the name of the machine. Defaults to the local machine name.
        /// </summary>
        /// <remarks>
        /// Fatal error to set it after we've already set everything up.
        /// </remarks>
        public static string MachineName
        {
            get
            {
                if (_machineName == null)
                {
                    _machineName = NetworkInformation.GetHostNames()
                        .Where(n => n.Type == Windows.Networking.HostNameType.DomainName).Select(x => x.DisplayName).FirstOrDefault();
                }
                return _machineName;
            }
            set
            {
                if (_MRUStream != null)
                {
                    throw new InvalidOperationException("Unable to udpate the MRUListUpdateStream.MachineName after we've initalized everything!");
                }
                _machineName = value;
            }
        }

        /// <summary>
        /// The machine name used for storing the local MRU list
        /// </summary>
        private static string _machineName = null;

        /// <summary>
        /// Returns an observable stream.
        /// </summary>
        /// <returns>The observable of the MRU listing</returns>
        /// <remarks>
        /// As soon as you subscribe you'll get the most recent version of the MRU list.
        /// 
        /// Combines MRU's from the following sources:
        /// 1. The internal database we maintain of our own stuff (that we've visited on this machine).
        /// </remarks>
        public static IObservable<IWalker.MRU[]> GetMRUListStream()
        {
            if (_MRUStream != null) {
                return _MRUStream;
            }

            // Get streams from all the sources we are going to have to deal with
            var locals = FetchMRUSFromLocalDB();
            var remotes = FetchMRUSFromRoamingData();

            // Combine the streams (without crossing them!!)
            var result = locals
                .CombineLatest(remotes, (l, r) => MergeStreams(l, r))
                .Select(mlst => mlst
                    .OrderByDescending(k => k.StartTime)
                    .Take(NumberOfMRUMeetings)
                    .ToArray())
                .Distinct(lst => MRUListHash(lst));

            // Make sure that it replays so when new folks join us (or a page gets re-initialized) they get the same
            // thing.
            _MRUStream = result
                .Replay(1)
                .RefCount();

            // When there is a MRU update, cache MRU to our remote file so others can see it.
            var db = new MRUDatabaseAccess();
            _MRUDBSubscription = MRUDatabaseAccess.MRUDBUpdated
                .SelectMany(_ => FetchMeetingsFromDB(db))
                .Subscribe(mrus =>
                {
                    MRUSettingsCache.UpdateForMachine(MachineName, mrus);
                });

            return _MRUStream;
        }

        /// <summary>
        /// Returns true if order and number are the same
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        private static int MRUListHash (IWalker.MRU[] list)
        {
            var bld = new StringBuilder();
            foreach (var l in list)
            {
                bld.Append(l.IDRef);
            }
            return bld.ToString().GetHashCode();
        }

        /// <summary>
        /// Make sure nothing gets GC'd
        /// </summary>
        private static IDisposable _MRUDBSubscription = null;

        /// <summary>
        /// Combine two lists
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        /// <remarks>
        /// Combines. When common items found, take the oldest start date.
        /// </remarks>
        private static IEnumerable<IWalker.MRU> MergeStreams(IWalker.MRU[] list1, IWalker.MRU[] list2)
        {
            var byMeeting = list1
                .Concat(list2)
                .GroupBy(k => k.IDRef);

            return byMeeting
                .Select(mgrp => mgrp.OrderByDescending(m => m.StartTime).First());
        }

        /// <summary>
        /// Fetch all the MRU's from our roaming data. Trigger an update each time
        /// there is a new update as well.
        /// </summary>
        /// <returns></returns>
        private static IObservable<IWalker.MRU[]> FetchMRUSFromRoamingData()
        {
            return MRUSettingsCache.RemoteMachineCacheUpdate
                .StartWith(default(Unit))
                .Select(_ => MRUSettingsCache.GetAllMachineMRUMeetings());
        }

        /// <summary>
        /// Generate a MRU[] stream from the local database. Each time it updates, we update here too.
        /// </summary>
        /// <returns></returns>
        private static IObservable<IWalker.MRU[]> FetchMRUSFromLocalDB()
        {
            // Generate an observable that will grab everything from our local MRU database.
            var m = new MRUDatabaseAccess();

            // Return the MRU updated guy
            return MRUDatabaseAccess.MRUDBUpdated
                .StartWith(default(Unit))
                .SelectMany(async _ =>
                {
                    IWalker.MRU[] r = await FetchMeetingsFromDB(m);

                    return r;
                });
        }

        /// <summary>
        /// Get the most recent 20 meetings from our local database
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static async Task<IWalker.MRU[]> FetchMeetingsFromDB(MRUDatabaseAccess m)
        {
            // Run this in the SQL engine
            var mruMeetings =
                (await m.QueryMRUDB())
                .OrderByDescending(mru => mru.LastLookedAt)
                .Take(NumberOfMRUMeetings)
                .ToListAsync();

            var r = (await mruMeetings)
                .Distinct(new MURCompare())
                .ToArray();
            return r;
        }
    }
}
