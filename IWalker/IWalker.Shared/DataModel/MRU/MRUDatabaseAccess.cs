using IWalker.DataModel.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.DataModel.MRU
{
    /// <summary>
    /// Dirt simple access to the MRU database, done a simple SQLite way.
    /// </summary>
    public class MRUDatabaseAccess : IMRUDatabase
    {
        /// <summary>
        /// Reference to our db class.
        /// </summary>
        SQLiteDb _db = null;

        /// <summary>
        /// Fires every time the MRU database is updated (even if it didn't change).
        /// </summary>
        public IObservable<Unit> MRUDBUpdated { get { return _mrusUpdated; } }

        /// <summary>
        /// Hold onto the trigger to fire when we have an update
        /// </summary>
        private Subject<Unit> _mrusUpdated;

        /// <summary>
        /// Connect up to our MRU database.
        /// </summary>
        public MRUDatabaseAccess()
        {
            _mrusUpdated = new Subject<Unit>();
        }

        /// <summary>
        /// Record this guy as having been visited.
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="startTime"></param>
        /// <param name="title"></param>
        public async Task MarkVisitedNow(IMeeting m)
        {
            await CreateDBConnection();

            // See if this meeting is already in the database. We have to
            // use the unique string for that, sadly.

            var mref = m.AsReferenceString();
            var entry = await (_db.AsyncConnection.Table<IWalker.MRU>().Where(mt => mt.IDRef == mref).FirstOrDefaultAsync());

            if (entry == null)
            {
                // Totally new!
                var mru = new IWalker.MRU()
                {
                    IDRef = mref,
                    StartTime = m.StartTime,
                    Title = m.Title,
                    LastLookedAt = DateTime.Now
                };
                var r = await _db.AsyncConnection.InsertAsync(mru);
                _mrusUpdated.OnNext(default(Unit));
            }
            else
            {
                // Just update a pre-existing object.
                entry.LastLookedAt = DateTime.Now;

                // And update all the other fields
                entry.Title = m.Title;
                entry.StartTime = m.StartTime;

                await _db.AsyncConnection.UpdateAsync(entry);
                _mrusUpdated.OnNext(default(Unit));
            }
        }

        /// <summary>
        /// Return a query of the database
        /// </summary>
        /// <returns></returns>
        public async Task<AsyncTableQuery<IWalker.MRU>> QueryMRUDB()
        {
            await CreateDBConnection();
            return _db.AsyncConnection.Table<IWalker.MRU>();
        }

        /// <summary>
        /// Execute some query asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<T> ExecuteScalarAsync<T> (string query, params object[] args)
        {
            await CreateDBConnection();
            return await _db.AsyncConnection.ExecuteScalarAsync<T>(query, args);
        }

        /// <summary>
        /// Get the db connection up and running.
        /// </summary>
        /// <returns></returns>
        private async Task CreateDBConnection()
        {
            if (_db == null)
                _db = await SQLiteDb.DB();
        }
    }
}
