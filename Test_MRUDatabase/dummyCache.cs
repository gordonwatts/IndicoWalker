using Akavache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase
{
    /// <summary>
    /// Helper class for testing - a dummy IBlobCache.
    /// </summary>
    class dummyCache : IBlobCache
    {
        private Dictionary<string, dummyCacheInfo> _lines;
        public class dummyCacheInfo
        {
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
            NumberTimesGetCalled = 0;
        }

        public int NumberTimesGetCalled { get; private set; }
        public int NumberTimesInsertCalled { get; set; }

        public IObservable<System.Reactive.Unit> Flush()
        {
            throw new NotImplementedException();
        }

        public IObservable<byte[]> Get(string key)
        {
            Debug.WriteLine("Trying to get data for {0}", key);
            NumberTimesGetCalled++;
            if (!_lines.ContainsKey(key))
            {
                Debug.WriteLine("  -> Nothing in cache.");
                return Observable.Throw<byte[]>(new KeyNotFoundException());
            }
            return Observable.Return(_lines[key].Data);
        }

        public IObservable<IEnumerable<string>> GetAllKeys()
        {
            throw new NotImplementedException();
        }

        public IObservable<DateTimeOffset?> GetCreatedAt(string key)
        {
            Debug.WriteLine("Trying to get object created at for key {0}", key);
            if (!_lines.ContainsKey(key))
            {
                Debug.WriteLine("  -> Nothing in cache.");
                return Observable.Return((DateTimeOffset?)null);
            }
            return Observable.Return<DateTimeOffset?>(_lines[key].DateCreated);
        }

        public IObservable<System.Reactive.Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = null)
        {
            NumberTimesInsertCalled++;
            Debug.WriteLine("Inserting data for key {0} - {1} bytes", key, data.Length);
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
