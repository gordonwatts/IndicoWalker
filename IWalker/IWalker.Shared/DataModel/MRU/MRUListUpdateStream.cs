using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive;

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
    /// </summary>
    public static class MRUListUpdateStream
    {
        /// <summary>
        /// Configure the number of meetings in the MRU list.
        /// </summary>
        const int NumberOfMRUMeetings = 20;

        static IObservable<IWalker.MRU[]> _MRUStream = null;

        /// <summary>
        /// Used only for testing!
        /// </summary>
        public static void Reset()
        {
            _MRUStream = null;
        }

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
        public static async Task<IObservable<IWalker.MRU[]>> GetMRUListStream()
        {
            if (_MRUStream != null) {
                return _MRUStream;
            }

            var result = await FetchMRUSFromLocalDB();

            // Make sure that it replays so when new folks join us (or a page gets re-initialized) they get the same
            // thing.

            _MRUStream = result
                .Replay(1)
                .RefCount();
            return _MRUStream;
        }

        /// <summary>
        /// Generate a MRU[] stream from the local database. Each time it updates, we update here too.
        /// </summary>
        /// <returns></returns>
        private static async Task<IObservable<IWalker.MRU[]>> FetchMRUSFromLocalDB()
        {
            // Generate an observable that will grab everything from our local MRU database.
            var m = new MRUDatabaseAccess();

            return m.MRUDBUpdated
                .StartWith(default(Unit))
                .SelectMany(async _ =>
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
                });
        }
    }
}
