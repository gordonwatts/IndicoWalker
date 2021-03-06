﻿using Akavache;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace IWalker.Util
{
    public static class AkavacheUtils
    {
        //
        // Summary:
        //     Attempt to return data from the cache. If the item doesn't exist or
        //     returns an error, call a Func to return the latest version of an object and
        //     insert the result in the cache.  For most Internet applications, this method
        //     is the best method to call to fetch static data (i.e. images) from the network.
        //
        // Parameters:
        //   key:
        //     The key to associate with the object.
        //
        //   fetchFunc:
        //     A Func which will asynchronously return the latest value for the bytes should
        //     the cache not contain the key. Observable.Start is the most straightforward
        //     way (though not the most efficient!) to implement this Func.
        //
        //   absoluteExpiration:
        //     An optional expiration date.
        //
        // Returns:
        //     A Future result representing the bytes from the cache.
        public static IObservable<byte[]> GetOrFetch(this IBlobCache This, string key, Func<IObservable<byte[]>> fetchFunc, DateTimeOffset? absoluteExpiration = null)
        {
            return This.Get(key)
                .Catch<byte[], Exception>(_ => Observable.Defer(() => fetchFunc())
                            .SelectMany(value => This.Insert(key, value, absoluteExpiration).Select(dummy => value)));
        }

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

            var items = fetchFromCache.Concat(fetchFromRemote);

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

            var items = fetchFromCache;

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

        /// <summary>
        /// Convenience routine to help with caching and extracting the page size from the cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="pageKey"></param>
        /// <param name="sizeCalcFunc"></param>
        /// <returns></returns>
        public static IObservable<IWalkerSize> GetOrFetchPageSize(this IBlobCache cache, string pageKey, Func<IObservable<IWalkerSize>> sizeCalcFunc)
        {
            var sizeKey = pageKey + "-DefaultPageSize";
            return cache.GetOrFetchObject<IWalkerSize>(sizeKey, sizeCalcFunc, DateTime.Now + Settings.PageCacheTime);
        }

        /// <summary>
        /// Helper function to cache or retrieve from the cache the image bytes
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="pageKey"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pageDataGenerator"></param>
        /// <returns></returns>
        public static IObservable<byte[]> GetOrFetchPageImageData(this IBlobCache cache, string pageKey, double width, double height, Func<IObservable<byte[]>> pageDataGenerator)
        {
            string pageDataKey = string.Format("{0}-w{1}-h{2}", pageKey, width, height);
            return cache.GetOrFetch(pageDataKey, pageDataGenerator, DateTime.Now + Settings.PageCacheTime);
        }
    }
}
