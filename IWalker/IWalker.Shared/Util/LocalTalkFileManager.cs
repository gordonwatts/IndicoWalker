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
        private static async Task<IRandomAccessStream> Download (this IFile file)
        {
            // Get the file stream, and write it out.
            var ms = new MemoryStream();
            using (var dataStream = await file.GetFileStream())
            {
                await dataStream.BaseStream.CopyToAsync(ms);
            }

            // First, get the place we are going to cache this file in the local database.
            var key = file.UniqueKey;
            var ar = ms.ToArray();
            Debug.WriteLine("Downloaded file of {0} bytes", ar.Length);
            await BlobCache.UserAccount.Insert(key, ar);
            return ar.AsRORAByteStream();
        }

        /// <summary>
        /// This will return a pointer to a file. If the file is in local storage, it will return that.
        /// If it isn't in local storage, it will fetch the file as long as checkForUpdates is true.
        /// </summary>
        /// <param name="file">The file we should fetch - from local storage or elsewhere. Null if it isn't local and can't be fetched.</param>
        /// <param name="checkForUpdates">If the file isn't present in local storage, get it from the IFile (remote resource)</param>
        public static IObservable<IRandomAccessStream> GetFile(this IFile file, bool checkForUpdates)
        {
            // Get it out of the local cache.
            var local = BlobCache.UserAccount.Get(file.UniqueKey)
                .Do(by => Debug.WriteLine("Got a file from cache of size {0} bytes", by.Length))
                .Select(by => by.AsRORAByteStream());

            // If it isn't there, then we should try to download it.
            var theFile = local
                .Catch<IRandomAccessStream, KeyNotFoundException>(e =>
                {
                    if (checkForUpdates) {
                        return Observable.FromAsync(_ => file.Download());
                    } else {
                        return Observable.Empty<IRandomAccessStream>();
                    }
                });

            return theFile;
        }
    }
}
