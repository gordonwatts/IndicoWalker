using IWalker.DataModel.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Connect up to our MRU database.
        /// </summary>
        public MRUDatabaseAccess()
        {
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
            var mru = new IWalker.MRU()
            {
                IDRef = "no way",
                StartTime = m.StartTime,
                Title = m.Title
            };
            var r = await _db.AsyncConnection.InsertAsync(mru);
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
