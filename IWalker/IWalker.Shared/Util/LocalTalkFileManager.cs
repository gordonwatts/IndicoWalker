using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace IWalker.Util
{
    /// <summary>
    /// Help with managing talk files and their local storage here on the system.
    /// </summary>
    public static class LocalTalkFileManager
    {
        /// <summary>
        /// Download a file. This ignores the state of the local cache, and will always download the file.
        /// It will save the newly downloaded file in the local cache.
        /// </summary>
        /// <param name="file"></param>
        public static void Download (this IFile file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This will return a pointer to a file. If the file is in local storage, it will return that.
        /// If it isn't in local storage, it will fetch the file as long as checkForUpdates is true.
        /// </summary>
        /// <param name="file">The file we should fetch - from local storage or elsewhere. Null if it isn't local and can't be fetched.</param>
        /// <param name="checkForUpdates">If the file isn't present in local storage, get it from the IFile (remote resource)</param>
        public static IObservable<IStorageFile> GetFile(this IFile file, bool checkForUpdates)
        {
            throw new NotImplementedException();
        }
    }
}
