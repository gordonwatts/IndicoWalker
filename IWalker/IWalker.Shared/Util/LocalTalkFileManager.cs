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
        /// Download a file to a local memory buffer. Save it locally when done as well.
        /// </summary>
        /// <param name="file"></param>
        private static async Task<Tuple<string, byte[]>> Download(this IFile file)
        {
            // Get the file stream, and write it out.
            var ms = new MemoryStream();
            using (var dataStream = await file.GetFileStream())
            {
                await dataStream.BaseStream.CopyToAsync(ms);
            }

            // Get the date from the header that we will need to stash
            var timeStamp = await file.GetFileDate();

            // This is what needs to be cached.
            var ar = ms.ToArray();
            return Tuple.Create(timeStamp, ar);
        }

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
        private static IObservable<bool> CheckForUpdate(this IFile file)
        {
            return Blobs.LocalStorage.GetObject<Tuple<string, byte[]>>(file.UniqueKey)
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
                    .Do(by => Debug.WriteLine("Got a file from cache of size {0} bytes", by.Item2.Length))
                    .Select(by => by.Item2.AsRORAByteStream())
                    .Catch<IRandomAccessStream, KeyNotFoundException>(e => Observable.Empty<IRandomAccessStream>());
        }

        /// <summary>
        /// This will return a pointer to a data stream representing the file. If the file is available in the cache, that
        /// will be returned first. It will then check to see if the file has been updated on the net. If so, it will download
        /// and re-cache that, and return a pointer to a data stream for the updated (or new) file.
        /// 
        /// In short, you can get anywhere between zero and two items in this sequence, depending on the
        /// arguments and the up-to-datedness of the file in the cache.
        /// </summary>
        /// <param name="file">The file we should fetch - from local storage or elsewhere. Null if it isn't local and can't be fetched.</param>
        /// <param name="requests">Each time this sequence fires, the file will be checked for a remote update and re-downloaded if it has been updated.</param>
        public static IObservable<IRandomAccessStream> GetAndUpdateFileOnce(this IFile file)
        {
            return Blobs.LocalStorage.GetAndRequestFetch(file.UniqueKey, () => Observable.FromAsync(() => file.Download()), dt => file.CheckForUpdate(), Observable.Return(default(Unit)), DateTime.Now + Settings.CacheFilesTime)
                .Select(a => a.Item2.AsRORAByteStream());
        }

        /// <summary>
        /// Update the file if it is either not cached, or out of date. Otherwise do nothing. And only attempt to update once.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static IObservable<IRandomAccessStream> UpdateFileOnce(this IFile file)
        {
            return file.CheckForUpdate()
                .Where(doUpdate => doUpdate == true)
                .SelectMany(_ => file.Download())
                .SelectMany(v => Blobs.LocalStorage.InsertObject(file.UniqueKey, v, DateTime.Now + Settings.CacheFilesTime).Select(_ => v))
                .Select(a => a.Item2.AsRORAByteStream())
                .Catch(Observable.Empty<IRandomAccessStream>());
        }

        /// <summary>
        /// If the file is present in the cache, return it right away. If not, wait until requests fires, and use that to update. A full update only happens if the file claims it is out of date.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="requests"></param>
        /// <returns></returns>
        public static IObservable<IRandomAccessStream> GetAndUpdateFileUponRequest(this IFile file, IObservable<Unit> requests)
        {
            return Blobs.LocalStorage.GetAndRequestFetch(file.UniqueKey, () => Observable.FromAsync(() => file.Download()), dt => file.CheckForUpdate(), requests, DateTime.Now + Settings.CacheFilesTime)
                .Select(a => a.Item2.AsRORAByteStream());
        }

        /// <summary>
        /// Returns the time that this particular objectw as put into the cache.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static IObservable<DateTimeOffset?> GetCacheCreateTime(this IFile file)
        {
            return Blobs.LocalStorage.GetObjectCreatedAt<Tuple<string,byte[]>>(file.UniqueKey);
        }
    }
}
