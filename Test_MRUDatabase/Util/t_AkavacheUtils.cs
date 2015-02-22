﻿using Akavache;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Reactive.Linq;
using IWalker.Util;
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
            await BlobCache.UserAccount.InvalidateAll();
        }

        [TestMethod]
        public async Task GetNewValueFetchTrue()
        {
            // Value is not in cache first.
            var rtn = await BlobCache.UserAccount.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(true))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await BlobCache.UserAccount.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetNewValueFetchFalse()
        {
            // Value is not in cache first. Event though we say no, it should still do the fetch.
            var rtn = await BlobCache.UserAccount.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(false))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("hi there", rtn[0]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await BlobCache.UserAccount.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetCachedValueFetchTrue()
        {
            // Value is in the cache, and we need to update it too.

            await BlobCache.UserAccount.InsertObject("key", "this is one");

            var rtn = await BlobCache.UserAccount.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(true))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(2, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);
            Assert.AreEqual("hi there", rtn[1]);

            // It should be in the cache.
            Assert.AreEqual("hi there", await BlobCache.UserAccount.GetObject<string>("key"));
        }

        [TestMethod]
        public async Task GetCachedValueFetchFalse()
        {
            // Value is in the cache, and we need to update it too.

            await BlobCache.UserAccount.InsertObject("key", "this is one");

            var rtn = await BlobCache.UserAccount.GetAndFetchLatest("key", () => Observable.Return("hi there"), dt => Observable.Return(false))
                .ToList()
                .FirstAsync();

            // We should have gotten it back
            Assert.IsNotNull(rtn);
            Assert.AreEqual(1, rtn.Count);
            Assert.AreEqual("this is one", rtn[0]);

            // It should be in the cache.
            Assert.AreEqual("this is one", await BlobCache.UserAccount.GetObject<string>("key"));
        }
    }
}