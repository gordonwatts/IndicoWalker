using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Reactive;
using System.Reactive.Linq;
using Windows.Storage.Streams;
using System.Diagnostics;

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
        private static async Task<Tuple<string,byte[]>> Download (this IFile file)
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
        /// <param name="file"></param>
        /// <param name="lastUpdated">The time our cache entry was created at</param>
        /// <returns>True once if we need to update again.</returns>
        /// <remarks>
        /// We use the date from the headers of the file to understand if we need to update. That way we don't have to deal
        /// with translating time-zones, and anything funny from indico.
        /// </remarks>
        private static IObservable<bool> CheckForUpdate(this IFile file)
        {
            return Blobs.LocalStorage.GetObject<Tuple<string,byte[]>>(file.UniqueKey)
                .Zip(Observable.FromAsync(() => file.GetFileDate()), (cacheDate, remoteDate) => cacheDate.Item1 != remoteDate);
        }

        /// <summary>
        /// This will return a pointer to a data stream representing the file, from cache. If there is nothing in cache, the empty
        /// sequence is returned.
        /// </summary>
        /// <param name="file">The file we should fetch - from local storage or elsewhere. Null if it isn't local and can't be fetched.</param>
        public static IObservable<IRandomAccessStream> GetFileFromCache(this IFile file)
        {
            return Blobs.LocalStorage.GetObject<Tuple<string, byte[]>>(file.UniqueKey)
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
        public static IObservable<IRandomAccessStream> GetAndUpdateFileOnce(this IFile file, IObservable<Unit> requests = null)
        {
            return Blobs.LocalStorage.GetAndFetchLatest(file.UniqueKey, () => Observable.FromAsync(() => file.Download()), dt => file.CheckForUpdate(), requests, DateTime.Now + Settings.CacheFilesTime)
                .Select(a => a.Item2.AsRORAByteStream());
        }
    }
}
