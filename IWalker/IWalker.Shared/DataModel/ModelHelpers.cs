using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel
{
    partial class Model
    {
        private static Lazy<SQLiteDb> _db = new Lazy<SQLiteDb>(() => new SQLiteDb("indico.db"));

        /// <summary>
        /// Return the singltone for the database model.
        /// </summary>
        public static SQLiteDb DB
        {
            get { return _db.Value; }
        }
    }
}
