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
        /// Record this guy as having been visited.
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="startTime"></param>
        /// <param name="title"></param>
        public async Task MarkVisitedNow(IMeeting m)
        {
            await Task.Delay(100); // Simulate the write. :-)
            Debug.WriteLine("Marking meeting {0} as being looked at", m.Title);
        }
    }
}
