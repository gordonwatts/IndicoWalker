using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace IWalker
{
    partial class SQLiteDb
    {
        /// <summary>
        /// Do the creation, but do it asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task CreateAsync()
        {
            var db = new SQLiteAsyncConnection(_path);
            await db.CreateTableAsync<IWalker.MRU>();
        }
        
        /// <summary>
        /// Name of the SQLite database we will be using.
        /// </summary>
        private const string _db_name = "indicowalker.db";

        internal static string DBPath
        {
            get
            {
                return string.Format(@"{0}\{1}", Windows.Storage.ApplicationData.Current.LocalFolder.Path, _db_name);
            }
        }

        /// <summary>
        /// Pointer to the helper classes generated for the database
        /// </summary>
        private static Lazy<SQLiteDb> _db = new Lazy<SQLiteDb>(() => new SQLiteDb(DBPath));

        /// <summary>
        /// Forget what we have known...
        /// </summary>
        public static void Forget()
        {
            if (_db.IsValueCreated)
            {
                _db.Value._asyncConnection = null;
            }
            _db = new Lazy<SQLiteDb>(() => new SQLiteDb(DBPath));
        }

        /// <summary>
        /// Drop and re-create the tables
        /// Used during testing.
        /// </summary>
        /// <returns></returns>
        internal static async Task ResetTables()
        {
            var dv = await DB();

            await dv.AsyncConnection.DropTableAsync<MRU>();

            await dv.CreateAsync();
        }

        /// <summary>
        /// Return the singleton for the database model.
        /// </summary>
        public static async Task<SQLiteDb> DB()
        {
            await AssureCreated();
            return _db.Value;
        }

        /// <summary>
        /// Get the DB asynchronous Connection to the database.
        /// </summary>
        public SQLiteAsyncConnection AsyncConnection
        {
            get
            {
                if (_asyncConnection == null)
                {
                    _asyncConnection = new SQLiteAsyncConnection(DBPath);
                }
                return _asyncConnection;
            }
        }
        private SQLiteAsyncConnection _asyncConnection = null;

        /// <summary>
        /// If the database hasn't been created yet, then create it.
        /// </summary>
        private static async Task AssureCreated()
        {
            // See it already exists.
            var lfs = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFilesAsync();
            if (lfs.Where(sf => sf.Name == _db_name).FirstOrDefault() != null)
                return;

            // Create the database.
            await _db.Value.CreateAsync();
        }
    }
}
