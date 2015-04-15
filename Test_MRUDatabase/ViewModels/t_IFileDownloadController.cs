using Akavache;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI.Testing;
using System.Threading.Tasks;
using ReactiveUI;
using System.IO;
using System.Reactive;

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
