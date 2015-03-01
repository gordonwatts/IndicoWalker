﻿using Akavache;
using IWalker.Util;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.Util
{
    [TestClass]
    public class t_AkavacheUtils
    {
        [TestInitialize]
        public async Task Setup()
        {
            BlobCache.ApplicationName = "Test_MRUDatabase";
            await Blobs.LocalStorage.InvalidateAll();
            await Blobs.LocalStorage.Flush();
        }

        [TestMethod]
        public async Task GetNewValueFetchTrue()
        {
            // Value is not in cache first.
            var rtn = await Blobs.LocalStorage.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(true))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetNewValueFetchFalse()
        {
            // Value is not in cache first. Event though we say no, it should still do the fetch.
            var rtn = await Blobs.LocalStorage.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(false))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetCachedValueFetchTrue()
        {
            // Value is in the cache, and we need to update it too.

            await Blobs.LocalStorage.InsertObject("key", "this is one");

            var rtn = await Blobs.LocalStorage.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(true))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(2, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetCachedValueFetchMultiTrue()
        {
            // Value is in the cache, and we need to update it too.

            await Blobs.LocalStorage.InsertObject("key", "this is one");

            var rtn = await Blobs.LocalStorage.GetAndFetchLatest("key", () => Observable.Return("hi there"), _ => Observable.Return(true), new Unit[] { default(Unit), default(Unit) }.ToObservable())
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(4, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);
            Assert.AreEqual("hi there", rtn[2]);
            Assert.AreEqual("hi there", rtn[3]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetNewValueFetchMultiTrue()
        {
            // Value is in the cache, and we need to update it too.

            var rtn = await Blobs.LocalStorage.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(true), new Unit[] { default(Unit), default(Unit) }.ToObservable())
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(3, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);
            Assert.AreEqual("hi there", rtn[2]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetNewValueWithDelayFetchMultiTrue()
        {
            // Value is in the cache, and we need to update it too.

            var rtn = await Blobs.LocalStorage.GetAndFetchLatest("key", () => Observable.Return("hi there").Delay(TimeSpan.FromMilliseconds(10)), dt => Observable.Return(true), new Unit[] { default(Unit), default(Unit) }.ToObservable())
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(3, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);
            Assert.AreEqual("hi there", rtn[2]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetCachedValueFetchFalse()
        {
            // Value is in the cache, and we need to update it too, but we shouldn't since the fetch function returns false.

            await Blobs.LocalStorage.InsertObject("key", "this is one");

            var rtn = await Blobs.LocalStorage.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(false))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);

            // It should be in the cache.
            Assert.AreEqual("this is one", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetNewValueRequestFetchTrue()
        {
            // Value is not in cache first.
            var rtn = await Blobs.LocalStorage.GetAndRequestFetch("key", () => Observable.Return("hi there"), dt => Observable.Return(true))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(0, rtn.Count);

            // It should not be in the cache.
            Assert.AreEqual("not there", await Blobs.LocalStorage.GetObject<string>("key").Catch(Observable.Return("not there")));
        }

        [TestMethod]
        public async Task GetNewValueRequestFetchFalse()
        {
            // Value is not in cache first. Since we say no, it should not do the fetch.
            var rtn = await Blobs.LocalStorage.GetAndRequestFetch("key", () => Observable.Return("hi there"), dt => Observable.Return(false))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(0, rtn.Count);
        }

        [TestMethod]
        public async Task GetCachedValueRequestFetchTrue()
        {
            // Value is in the cache, and we need to update it too.

            await Blobs.LocalStorage.InsertObject("key", "this is one");

            var rtn = await Blobs.LocalStorage.GetAndRequestFetch("key", () => Observable.Return("hi there"), dt => Observable.Return(true))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);

            // It should be in the cache, but not updated
            Assert.AreEqual("this is one", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetCachedValueRequestFetchMultiTrue()
        {
            // Value is in the cache, and we need to update it too.

            await Blobs.LocalStorage.InsertObject("key", "this is one");

            var rtn = await Blobs.LocalStorage.GetAndRequestFetch("key", () => Observable.Return("hi there"), _ => Observable.Return(true), new Unit[] { default(Unit), default(Unit) }.ToObservable())
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(3, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);
            Assert.AreEqual("hi there", rtn[2]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetNewValueRequestFetchMultiTrue()
        {
            // Value isn't in the cache, and we need to run the update.

            var rtn = await Blobs.LocalStorage.GetAndRequestFetch("key", () => Observable.Return("hi there"), dt => Observable.Return(true), new Unit[] { default(Unit), default(Unit) }.ToObservable())
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(2, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetNewValueWithDelayRequestFetchMultiTrue()
        {
            // Value is not in the cache, and we need to update it too.

            var rtn = await Blobs.LocalStorage.GetAndRequestFetch("key", () => Observable.Return("hi there").Delay(TimeSpan.FromMilliseconds(10)), dt => Observable.Return(true), new Unit[] { default(Unit), default(Unit) }.ToObservable())
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(2, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await Blobs.LocalStorage.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetCachedValueRequestFetchFalse()
        {
            // Value is in the cache, and we need to update it too, but we shouldn't since the fetch function returns false.

            await Blobs.LocalStorage.InsertObject("key", "this is one");

            var rtn = await Blobs.LocalStorage.GetAndRequestFetch("key", () => Observable.Return("hi there"), dt => Observable.Return(false))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);

            // It should be in the cache.
            Assert.AreEqual("this is one", await Blobs.LocalStorage.GetObject<string>("key"));
        }
    }
}
