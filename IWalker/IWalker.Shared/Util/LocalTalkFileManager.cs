using Akavache;
using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace IWalker.Util
{
    /// <summary>
    /// Help with managing talk files and their local storage here on the system.
    /// </summary>
    public static class LocalTalkFileManager
    {
        /// <summary>
        /// Return the key we will use to lookup to see if the file has been downloaded recently.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string FileDateKey(this IFile file)
        {
            return string.Format("{0}-file-date", file.UniqueKey);
        }

        /// <summary>
        /// Check a file to see if it needs to be re-downloaded.
        /// </summary>
        /// <param name="file">The file to look at for updates, which must already exist in the blob cache</param>
        /// <returns>True once if we need to update again.</returns>
        /// <remarks>
        /// We use the date from the headers of the file to understand if we need to update. That way we don't have to deal
        /// with translating time-zones, and anything funny from indico.
        /// 
        /// If something goes wrong (e.g. we are offline), the also return false.
        /// If the cache is missing the item, then we need to update.
        /// </remarks>
        public static IObservable<bool> CheckForUpdate(this IFile file, IBlobCache cache)
        {
            return cache.GetObject<Tuple<string, byte[]>>(file.UniqueKey)
                .Zip(file.GetFileDate(), (cacheDate, remoteDate) => cacheDate.Item1 != remoteDate)
                .Catch<bool, KeyNotFoundException>(_ => Observable.Return(true))
                .Catch(Observable.Return(false));
        }

        /// <summary>
        /// This will return a pointer to a data stream representing the file, from cache. If there is nothing in cache, the empty
        /// sequence is returned.
        /// </summary>
        /// <param name="file">The file we should fetch - from local storage or elsewhere. Null if it isn't local and can't be fetched.</param>
        public static IObservable<IRandomAccessStream> GetFileFromCache(this IFile file, IBlobCache cache)
        {
            return cache.GetObject<Tuple<string, byte[]>>(file.UniqueKey)
                    .Select(by => by.Item2.AsRORAByteStream())
                    .Catch<IRandomAccessStream, KeyNotFoundException>(e => Observable.Empty<IRandomAccessStream>());
        }

        /// <summary>
        /// Save all the data we pull off the internet in the cache for later user.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="serverModifiedType"></param>
        /// <param name="filedata"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static IObservable<Unit> SaveFileInCache(this IFile file, string serverModifiedType, byte[] filedata, IBlobCache cache)
        {
            return cache.InsertObject(file.UniqueKey, Tuple.Create(serverModifiedType, filedata), DateTime.Now + Settings.CacheFilesTime);
        }

        /// <summary>
        /// Returns the time that this particular object was put into the cache.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static IObservable<DateTimeOffset?> GetCacheCreateTime(this IFile file, IBlobCache cache = null)
        {
            cache = cache ?? Blobs.LocalStorage;
            return cache.GetObjectCreatedAt<Tuple<string, byte[]>>(file.UniqueKey);
        }
    }
}
