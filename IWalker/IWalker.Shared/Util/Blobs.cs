using Akavache;
using Akavache.Sqlite3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if BACKGROUND_LIBRARY
namespace IWalker.BackgroundTasks
#else
namespace IWalker.Util
#endif
{
#if BACKGROUND_LIBRARY
    class Blobs
#else
    public class Blobs
#endif
    {
        /// <summary>
        /// Return the blob cache for the main local storage.
        /// </summary>
        public static IBlobCache LocalStorage
        {
            get { Register(); return _localStorageBlobCache.Value; }
        }
        private static Lazy<IBlobCache> _localStorageBlobCache = null;

        /// <summary>
        /// We need a blob cache for our stuff, but located in local directory rather than the roaming profile.
        /// This isn't provided in Akavache, so I thought I'd do it here.
        /// </summary>
        public static void Register()
        {
            if (_localStorageBlobCache == null)
            {
                _localStorageBlobCache = new Lazy<IBlobCache>(() =>
                {
                    var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    return new SQLitePersistentBlobCache(Path.Combine(folder.Path, "localblobs.db"), BlobCache.TaskpoolScheduler);
                });
            }
        }

        /// <summary>
        /// Clean up everything - flush, etc.
        /// </summary>
        private static void Shutdown()
        {
            if (_localStorageBlobCache.IsValueCreated)
            {
                _localStorageBlobCache.Value.Dispose();
            }
        }
    }
}
