using Akavache;
using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ReactiveUI;
using ReactiveUI.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_IFileDownloadController
    {
        [TestInitialize]
        public void TestSetup()
        {

        }

        [TestMethod]
        public void NoFileDownloadedIsNotCached()
        {
            var f = new dummyFile();
            var vm = new IFileDownloadController(f, new dummyCache());
            var dummy = vm.IsDownloaded;
            Assert.IsFalse(vm.IsDownloaded);
        }

        [TestMethod]
        public void FileDownlaodedIsCached()
        {
            var f = new dummyFile();
            var dc = new dummyCache();
            dc.InsertObject(f.UniqueKey, Tuple.Create(DateTime.Now.ToString(), new byte[] { 0, 1 }));
            var vm = new IFileDownloadController(f, dc);
            var dummy = vm.IsDownloaded;
            
            Assert.IsTrue(vm.IsDownloaded);
        }

        [TestMethod]
        public void TriggerSuccessfulFileDownloadNoCache()
        {
            var f = new dummyFile();

            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            f.GetStream = () => Observable.Return(new StreamReader(mr));

            var vm = new IFileDownloadController(f, new dummyCache());
            var dummy = vm.IsDownloaded;

            vm.DownloadOrUpdate.Execute(null);
            Assert.IsTrue(vm.IsDownloaded);
        }

        [TestMethod]
        public void TriggerSuccessfulFileDownloadCached()
        {
            var f = new dummyFile();

            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            f.GetStream = () => Observable.Return(new StreamReader(mr));

            var dc = new dummyCache();
            dc.InsertObject(f.UniqueKey, Tuple.Create(DateTime.Now, new byte[] { 0, 1, 2 }));
            var vm = new IFileDownloadController(f, dc);
            var dummy = vm.IsDownloaded;

            vm.DownloadOrUpdate.Execute(null);
            Assert.IsTrue(vm.IsDownloaded);
        }

        [TestMethod]
        public void DownloadInProgressIsSet()
        {
            var f = new dummyFile();

            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            f.GetStream = () => Observable.Return(new StreamReader(mr));

            var dc = new dummyCache();
            var vm = new IFileDownloadController(f, dc);
            int isDownloadingCounter = 0;
            vm.WhenAny(x => x.IsDownloading, x => x.Value)
                .Subscribe(_ => isDownloadingCounter++);
            var dummy = vm.IsDownloaded;
            var dummy1 = vm.IsDownloading;

            vm.DownloadOrUpdate.Execute(null);

            // It should have gone from false to true, and back to false.
            Assert.AreEqual(2, isDownloadingCounter);
        }

        [TestMethod]
        public async Task IsDownloadingFlipsCorrectly()
        {
            await new TestScheduler().With(async sched =>
            {
                // http://stackoverflow.com/questions/21588945/structuring-tests-or-property-for-this-reactive-ui-scenario
                var f = new dummyFile();

                f.GetStream = () =>
                {
                    var data = new byte[] { 0, 1, 2, 3 };
                    var mr = new MemoryStream(data);
                    return Observable.Return(new StreamReader(mr)).WriteLine("created stream reader").Delay(TimeSpan.FromMilliseconds(100), sched).WriteLine("done with delay for stream reader");
                };

                var dc = new dummyCache();
                var vm = new IFileDownloadController(f, dc);
                var dummy = vm.IsDownloaded;
                var dummy1 = vm.IsDownloading;

                Assert.IsFalse(vm.IsDownloading);
                vm.DownloadOrUpdate.Execute(null);

                // Since the download is synchronous, it should get here just fine.
                sched.AdvanceByMs(10);
                Assert.IsTrue(vm.IsDownloading);
                Assert.IsFalse(vm.IsDownloaded);

                // And run past the end
                sched.AdvanceByMs(200);

                //TODO: Not clear why this is required (the delay), but it is!
                await Task.Delay(200);
                Assert.IsTrue(vm.IsDownloaded);
                Assert.IsFalse(vm.IsDownloading);
            });
        }

        [TestMethod]
        public void DownloadCalledOnceOnNewFile()
        {
            var f = new dummyFile();

            f.GetStream = () =>
            {
                var data = new byte[] { 0, 1, 2, 3 };
                var mr = new MemoryStream(data);
                return Observable.Return(new StreamReader(mr));
            };

            var dc = new dummyCache();
            var vm = new IFileDownloadController(f, dc);
            int isDownloadingCounter = 0;
            vm.WhenAny(x => x.IsDownloading, x => x.Value)
                .Subscribe(_ => isDownloadingCounter++);
            var dummy = vm.IsDownloaded;
            var dummy1 = vm.IsDownloading;

            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(1, f.GetStreamCalled);
        }

        [TestMethod]
        public void DownloadCalledOnceOnCacheUpdate()
        {
            var f = new dummyFile();

            f.GetStream = () =>
            {
                var data = new byte[] { 0, 1, 2, 3 };
                var mr = new MemoryStream(data);
                return Observable.Return(new StreamReader(mr));
            };

            var dc = new dummyCache();
            dc.InsertObject(f.UniqueKey, Tuple.Create(DateTime.Now.ToString(), new byte[] { 0, 1 }));
            var vm = new IFileDownloadController(f, dc);
            int isDownloadingCounter = 0;
            vm.WhenAny(x => x.IsDownloading, x => x.Value)
                .Subscribe(_ => isDownloadingCounter++);
            var dummy = vm.IsDownloaded;
            var dummy1 = vm.IsDownloading;

            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(1, f.GetStreamCalled);
        }

        [TestMethod]
        public void DownloadNotCalledOnceOnCacheUpdate()
        {
            var f = new dummyFile();

            f.GetStream = () =>
            {
                var data = new byte[] { 0, 1, 2, 3 };
                var mr = new MemoryStream(data);
                return Observable.Return(new StreamReader(mr));
            };

            var dc = new dummyCache();
            var theDateString = "this is now";
            dc.InsertObject(f.UniqueKey, Tuple.Create(theDateString, new byte[] { 0, 1 }));
            f.DateToReturn = theDateString;
            var vm = new IFileDownloadController(f, dc);

            int isDownloadingCounter = 0;
            vm.WhenAny(x => x.IsDownloading, x => x.Value)
                .Subscribe(_ => isDownloadingCounter++);
            var dummy = vm.IsDownloaded;
            var dummy1 = vm.IsDownloading;

            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(0, f.GetStreamCalled);
        }

        /// <summary>
        /// Make sure the get data is not called too much (since it will generate a web
        /// request).
        /// </summary>
        [TestMethod]
        public void CheckDateCalledOnceCacheEmpty()
        {
            Assert.Inconclusive();
        }

        /// <summary>
        /// Make sure the get data is not called too much (since it will generate a web
        /// request).
        /// </summary>
        [TestMethod]
        public void CheckDateCalledOnceCacheFilled()
        {
            Assert.Inconclusive();
        }

        /// <summary>
        /// Does a simple ReactiveCommand actually need a scheduler?
        /// No: the below works just fine.
        /// </summary>
        [TestMethod]
        public void MakeSureReactiveCommandWorks()
        {
            var rc = ReactiveCommand.CreateAsyncObservable(_ => Observable.Return(false));
            var value = true;
            rc.Subscribe(v => value = v);
            Assert.IsTrue(value);
            rc.Execute(null);
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void MakeSureReactiveCommandWorks2()
        {
            var rc = ReactiveCommand.CreateAsyncObservable(_ => Observable.Return(false).Select(dummy => false));
            var value = true;
            rc.Subscribe(v => value = v);
            Assert.IsTrue(value);
            rc.Execute(null);
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void UnderstandHowDummyCacheCausesProblems()
        {
            var f = new dummyFile();
            var dc = new dummyCache();
            var DownloadOrUpdate = ReactiveCommand.CreateAsyncObservable(_ =>
                dc.GetCreatedAt(f.UniqueKey)
                .Select(dt => dt.HasValue));

            var value = false;
            DownloadOrUpdate.Subscribe(_ => value = true);
            Assert.IsFalse(value);
            DownloadOrUpdate.Execute(null);
            Assert.IsTrue(value);
        }

        private class dummyCache : IBlobCache
        {
            private Dictionary<string, dummyCacheInfo> _lines;
            public class dummyCacheInfo {
                public DateTime DateCreated;
                public byte[] Data = null;
            }

            public dummyCache(Dictionary<string, dummyCacheInfo> lines = null)
            {
                _lines = lines;
                if (_lines == null)
                {
                    _lines = new Dictionary<string, dummyCacheInfo>();
                }
            }
            public IObservable<System.Reactive.Unit> Flush()
            {
                throw new NotImplementedException();
            }

            public IObservable<byte[]> Get(string key)
            {
                if (!_lines.ContainsKey(key))
                {
                    throw new KeyNotFoundException();
                }
                return Observable.Return(_lines[key].Data);
            }

            public IObservable<IEnumerable<string>> GetAllKeys()
            {
                throw new NotImplementedException();
            }

            public IObservable<DateTimeOffset?> GetCreatedAt(string key)
            {
                if (!_lines.ContainsKey(key))
                {
                    return Observable.Return((DateTimeOffset?)null);
                }
                return Observable.Return<DateTimeOffset?>(_lines[key].DateCreated);
            }

            public IObservable<System.Reactive.Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = null)
            {
                _lines[key] = new dummyCacheInfo() { DateCreated = DateTime.Now, Data = data };
                return Observable.Return(default(Unit));
            }

            public IObservable<System.Reactive.Unit> Invalidate(string key)
            {
                throw new NotImplementedException();
            }

            public IObservable<System.Reactive.Unit> InvalidateAll()
            {
                throw new NotImplementedException();
            }

            public System.Reactive.Concurrency.IScheduler Scheduler
            {
                get { throw new NotImplementedException(); }
            }

            public IObservable<System.Reactive.Unit> Shutdown
            {
                get { throw new NotImplementedException(); }
            }

            public IObservable<System.Reactive.Unit> Vacuum()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

    }
}
