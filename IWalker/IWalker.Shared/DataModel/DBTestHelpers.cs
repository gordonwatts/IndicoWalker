using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace IWalker.DataModel
{
    /// <summary>
    /// Should only be used during testing
    /// </summary>
    public class DBTestHelpers
    {
        /// <summary>
        /// Really only for testing. Will delete the db! All gone! No recovery!
        /// </summary>
        /// <returns></returns>
        public static async Task DeleteDB()
        {
            try
            {
                SQLiteDb.Forget();
                GC.Collect();
                var f = await StorageFile.GetFileFromPathAsync(SQLiteDb.DBPath);
                await f.DeleteAsync();
                return;
            }
            catch (FileNotFoundException e) {
                return;
            }
            catch (UnauthorizedAccessException e)
            {
            }

            // We couldn't delete the file, so we should re-create everything.
            await SQLiteDb.ResetTables();
        }

    }
}
