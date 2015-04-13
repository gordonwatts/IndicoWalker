using Akavache;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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
            Assert.IsFalse(vm.IsDownloaded);
        }

        [TestMethod]
        public void FielDownlaodedIsCached()
        {
            var f = new dummyFile();
            var d = new Dictionary<string, dummyCache.dummyCacheInfo>();
            d[f.UniqueKey] = new dummyCache.dummyCacheInfo() { DateCreated = DateTime.Now };
            var vm = new IFileDownloadController(f, new dummyCache(d));
            
            Assert.IsTrue(vm.IsDownloaded);
        }

        private class dummyCache : IBlobCache
        {
            private Dictionary<string, dummyCacheInfo> _lines;
            public class dummyCacheInfo {
                public DateTime DateCreated;
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
