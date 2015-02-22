using Akavache;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.Util
{
    public static class AkavacheUtils
    {
        /// <summary>
        /// This method attempts to returned a cached value, and fetch one from
        /// the web.
        /// 
        /// If the cached value doesn't exist, then it must be supplied from
        /// the web.
        /// 
        /// If the value does exist, a separate check function will be called
        /// to re-fetch from the web.
        /// 
        /// This method returns an IObservable that may return more than one result
        /// (first the cached data, then the latest data, and if your fetchPredicate
        /// returns multiple values). Therefore, it's
        /// important for UI applications that in your Subscribe method, you
        /// write the code to merge the second result when it comes in.
        ///
        /// This also means that await'ing this method is a Bad Idea(tm), always
        /// use Subscribe.
        /// </summary>
        /// <param name="key">The key to store the returned result under.</param>
        /// <param name="fetchFunc">A sequence that will return the new values of the data</param>
        /// <param name="fetchPredicate">A Func to determine whether
        /// the updated item should be fetched. If the cached version isn't found,
        /// this parameter is ignored and the item is always fetched. It is possible to return multiple
        /// items in this sequence, in which case the fetch will repeat multiple times.</param>
        /// <param name="absoluteExpiration">An optional expiration date.</param>
        /// <param name="shouldInvalidateOnError">If this is true, the cache will
        /// be cleared when an exception occurs in fetchFunc</param>
        /// <returns>An Observable stream containing either one or two
        /// results (possibly a cached version, then the latest version)</returns>
        public static IObservable<T> GetAndFetchLatest<T>(this IBlobCache This,
            string key,
            Func<IObservable<T>> fetchFunc,
            Func<DateTimeOffset, IObservable<bool>> fetchPredicate)
        {
            if (fetchPredicate == null)
                throw new ArgumentException("fetchPredicate");
            if (fetchFunc == null)
                throw new ArgumentException("fetchFunc");

            // We are going to get the cache value if we can. And then we will run updates after that.
            // If we have nothing cached, then we will run the fetch directly. Otherwise we will run the
            // fetch sequence.

            var getOldKey = Observable.Defer(() => This.GetObjectCreatedAt<T>(key));

            var refetchIfNeeded = getOldKey
                .Where(dt => dt != null && dt.HasValue && dt.Value != null)
                .SelectMany(dt => fetchPredicate(dt.Value))
                .Where(doit => doit == true)
                .Select(_ => default(Unit));

            var fetchRequired = getOldKey
                .Where(dt => dt == null || !dt.HasValue || dt.Value == null)
                .Select(_ => default(Unit));

            // Next, get the item...

            var fetchFromCache = Observable.Defer(() => This.GetObject<T>(key))
                .Catch<T, KeyNotFoundException>(_ => Observable.Empty<T>());

            var fetchFromRemote = refetchIfNeeded.Concat(fetchRequired)
                .SelectMany(_ => fetchFunc())
                .SelectMany(x => This.InsertObject<T>(key, x).Select(_ => x));

            return fetchFromCache.Concat(fetchFromRemote).Multicast(new ReplaySubject<T>()).RefCount();
        }
    }
}
