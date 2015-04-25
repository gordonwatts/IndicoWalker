using Akavache;
using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ReactiveUI;
using ReactiveUI.Testing;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_FileDownloadController
    {
        [TestMethod]
        public void NoFileDownloadedIsNotCached()
        {
            var f = new dummyFile();
            var vm = new FileDownloadController(f, new dummyCache());
            var dummy = vm.IsDownloaded;
            Assert.IsFalse(vm.IsDownloaded);
        }

        [TestMethod]
        public void FileDownlaodedIsCached()
        {
            var f = new dummyFile();
            var dc = new dummyCache();
            dc.InsertObject(f.UniqueKey, Tuple.Create(DateTime.Now.ToString(), new byte[] { 0, 1 }));
            var vm = new FileDownloadController(f, dc);
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

            var vm = new FileDownloadController(f, new dummyCache());
            var dummy = vm.IsDownloaded;

            int downloadUpdateCount = 0;
            vm.FileDownloadedAndCached.Subscribe(_ => downloadUpdateCount++);

            vm.DownloadOrUpdate.Execute(null);
            Assert.IsTrue(vm.IsDownloaded);
            Assert.AreEqual(1, downloadUpdateCount);
        }

        [TestMethod]
        public async Task TriggerSuccessfulFIleDownloadCheckLate()
        {
            var f = new dummyFile();

            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            f.GetStream = () => Observable.Return(new StreamReader(mr));

            var vm = new FileDownloadController(f, new dummyCache());

            int downloadUpdateCount = 0;
            vm.FileDownloadedAndCached.Subscribe(_ => downloadUpdateCount++);

            vm.DownloadOrUpdate.Execute(null);

            // ReactiveUI won't cause a "subscription" until this is first accessed.
            // Make sure that it reflects the fact that it is downloaded, even though
            // the trigger for this happens earlier.
            var bogus = vm.IsDownloaded;
            await Task.Delay(10);
            Assert.IsTrue(vm.IsDownloaded);
            Assert.AreEqual(1, downloadUpdateCount);
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
            var vm = new FileDownloadController(f, dc);
            var dummy = vm.IsDownloaded;

            int downloadUpdateCount = 0;
            vm.FileDownloadedAndCached.Subscribe(_ => downloadUpdateCount++);

            vm.DownloadOrUpdate.Execute(null);
            Assert.IsTrue(vm.IsDownloaded);
            Assert.AreEqual(1, downloadUpdateCount);
        }

        [TestMethod]
        public void TriggerNoFileDownloadCached()
        {
            var f = new dummyFile();

            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            f.GetStream = () => Observable.Return(new StreamReader(mr));

            var dc = new dummyCache();
            dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, new byte[] { 0, 1, 2 }));
            var vm = new FileDownloadController(f, dc);
            var dummy = vm.IsDownloaded;

            int downloadUpdateCount = 0;
            vm.FileDownloadedAndCached.Subscribe(_ => downloadUpdateCount++);

            vm.DownloadOrUpdate.Execute(null);
            Assert.IsTrue(vm.IsDownloaded);
            Assert.AreEqual(0, downloadUpdateCount);
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
                var vm = new FileDownloadController(f, dc);
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
            var vm = new FileDownloadController(f, dc);
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
            var vm = new FileDownloadController(f, dc);
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
            var vm = new FileDownloadController(f, dc);

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
            var f = new dummyFile();

            f.GetStream = () =>
            {
                var data = new byte[] { 0, 1, 2, 3 };
                var mr = new MemoryStream(data);
                return Observable.Return(new StreamReader(mr));
            };

            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);
            int isDownloadingCounter = 0;
            vm.WhenAny(x => x.IsDownloading, x => x.Value)
                .Subscribe(_ => isDownloadingCounter++);
            var dummy = vm.IsDownloaded;
            var dummy1 = vm.IsDownloading;

            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(1, f.GetDateCalled);
        }

        /// <summary>
        /// Make sure the get data is not called too much (since it will generate a web
        /// request).
        /// </summary>
        [TestMethod]
        public void CheckDateCalledOnceCacheFilled()
        {
            var f = new dummyFile();

            f.GetStream = () =>
            {
                var data = new byte[] { 0, 1, 2, 3 };
                var mr = new MemoryStream(data);
                return Observable.Return(new StreamReader(mr));
            };

            var dc = new dummyCache();
            dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, new byte[] { 0, 1, 2 }));
            var vm = new FileDownloadController(f, dc);
            int isDownloadingCounter = 0;
            vm.WhenAny(x => x.IsDownloading, x => x.Value)
                .Subscribe(_ => isDownloadingCounter++);
            var dummy = vm.IsDownloaded;
            var dummy1 = vm.IsDownloading;

            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(1, f.GetDateCalled);
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

        [TestMethod]
        public void CtorHasNothingAccessedNoCache()
        {
            var f = new dummyFile();
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            Assert.AreEqual(0, f.GetDateCalled);
            Assert.AreEqual(0, f.GetStreamCalled);
        }

        [TestMethod]
        public void CtorHasNothingAccessedCache()
        {
            var f = new dummyFile();
            var dc = new dummyCache();
            dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, new byte[] { 0, 1, 2, 3 }));
            var vm = new FileDownloadController(f, dc);

            Assert.AreEqual(0, f.GetDateCalled);
            Assert.AreEqual(0, f.GetStreamCalled);
        }

    }
}
