using IWalker.DataModel.Interfaces;
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
            _db = Model.DB;
        }

        /// <summary>
        /// Record this guy as having been visited.
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="startTime"></param>
        /// <param name="title"></param>
        public async Task MarkVisitedNow(IMeeting m)
        {
            var c = new SQLite.SQLiteAsyncConnection("dude");
            var mru = new IWalker.MRU()
            {
                IDRef = "no way",
                StartTime = m.StartTime,
                Title = m.Title
            };
            var r = await c.InsertAsync(mru);

            await Task.Delay(100); // Simulate the write. :-)
            Debug.WriteLine("Marking meeting {0} as being looked at", m.Title);
        }
    }
}
