//This code was generated by a tool.
//Changes to this file will be lost if the code is regenerated.
using SQLite;
using System;

namespace IWalker
{
    partial class SQLiteDb
    {
        string _path;
        public SQLiteDb(string path)
        {
            _path = path;
        }
        
         public void Create()
        {
            using (SQLiteConnection db = new SQLiteConnection(_path))
            {
                db.CreateTable<MRU>();
            }
        }
    }
    
    public partial class MRU
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        
        [MaxLength(40)]
        [NotNull]
        public String Title { get; set; }
        
        [NotNull]
        public DateTime StartTime { get; set; }

        [NotNull]
        public DateTime LastLookedAt { get; set; }
        
        [MaxLength(256)]
        [NotNull]
        public String IDRef { get; set; }
        
    }
    
}
