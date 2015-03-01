using Akavache;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace IWalker.Util
{
    public static class AkavacheUtils
    {
        /// <summary>
        /// This method attempts to returned a cached value, and fetch one from
        /// the web. Optionally, it can continue to query to see if an update is required.
        /// 
        /// If the cached value exists, it is returned. Then the predicate is queried to see
        /// if the remote value should be refreshed.
        /// 
        /// If there is no cached value, then the value is fetched.
        /// 
        /// Once the above is done, as the retrySequence comes in, the predicate will
        /// be called to see if a refresh is needed. If so, the data will be re-fetched.
        /// 
        /// In all cases any remotely fetched data is cached.
        /// 
        /// This also means that await'ing this method is a Bad Idea(tm), always
        /// use Subscribe. 1-infinity values can be returned depending on the arguments.
        /// </summary>
        /// <param name="key">The key to store the returned result under.</param>
        /// <param name="fetchFunc">A sequence that will return the new values of the data</param>
        /// <param name="fetchPredicate">A Func to determine whether
        /// the updated item should be fetched. Only called once a cached version exists.</param>
        /// <param name="retrySequence">Sequence that will trigger a predicate call followed by
        /// a fetch call if the predicate indicates so.</param>
        /// <param name="This">The blob cache against which we operate</param>
        /// <returns>An Observable stream containing one or more
        /// results (possibly a cached version, then the latest version(s))</returns>
        public static IObservable<T> GetAndFetchLatest<T>(this IBlobCache This,
            string key,
            Func<IObservable<T>> fetchFunc,
            Func<DateTimeOffset, IObservable<bool>> fetchPredicate,
            IObservable<Unit> retrySequence = null,
            DateTimeOffset? absoluteExpiration = null
            )
        {
            if (fetchPredicate == null)
                throw new ArgumentException("fetchPredicate");
            if (fetchFunc == null)
                throw new ArgumentException("fetchFunc");

            // We are going to get the cache value if we can. And then we will run updates after that.
            // If we have nothing cached, then we will run the fetch directly. Otherwise we will run the
            // fetch sequence.

            var getOldKey = This.GetObjectCreatedAt<T>(key);

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

            var fetchFromRemote = fetchRequired.Concat(refetchIfNeeded)
                .SelectMany(_ => fetchFunc())
                .SelectMany(x => This.InsertObject<T>(key, x, absoluteExpiration).Select(_ => x));

            var items = fetchFromCache.Concat(fetchFromRemote).Multicast(new ReplaySubject<T>()).RefCount();

            // Once we have these, we also have to kick off a second set of fetches for our retry sequence.
            if (retrySequence == null)
                return items;

            var getAfter = retrySequence
                .SelectMany(_ => This.GetObjectCreatedAt<T>(key))
                .SelectMany(dt => fetchPredicate(dt.Value))
                .Where(doit => doit == true)
                .SelectMany(_ => fetchFunc())
                .SelectMany(x => This.InsertObject<T>(key, x, absoluteExpiration).Select(_ => x));

            return items.Concat(getAfter);

        }

        public static IObservable<T> GetAndRequestFetch<T>(this IBlobCache This,
            string key,
            Func<IObservable<T>> fetchFunc,
            Func<DateTimeOffset?, IObservable<bool>> fetchPredicate,
            IObservable<Unit> requestSequence = null,
            DateTimeOffset? absoluteExpiration = null
            )
        {
            if (fetchPredicate == null)
                throw new ArgumentException("fetchPredicate");
            if (fetchFunc == null)
                throw new ArgumentException("fetchFunc");

            // We are going to get the cache value if we can. And then we will run updates after that.
            // If we have nothing cached, then we will run the fetch directly. Otherwise we will run the
            // fetch sequence.

            var getOldKey = This.GetObjectCreatedAt<T>(key);

            // Next, get the item...

            var fetchFromCache = Observable.Defer(() => This.GetObject<T>(key))
                .Catch<T, KeyNotFoundException>(_ => Observable.Empty<T>());

            var items = fetchFromCache.Multicast(new ReplaySubject<T>()).RefCount();

            // Once we have these, we also have to kick off a second set of fetches for our retry sequence.
            if (requestSequence == null)
                return items;

            // TODO: How can we make this atomic. THe problem is that fetchPredicate may depend on the object having been
            // inserted, but because they are on different threads we may get race conditions. So there must be a way...
            var getAfter = requestSequence
                .SelectMany(_ => This.GetObjectCreatedAt<T>(key))
                .SelectMany(dt => fetchPredicate(dt))
                .Where(doit => doit == true)
                .SelectMany(_ => fetchFunc())
                .SelectMany(x => This.InsertObject<T>(key, x, absoluteExpiration).Select(_ => x));

            return items.Concat(getAfter);

        }
    }
}
