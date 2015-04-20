using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using Akavache;
using ReactiveUI.Testing;
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using System.Diagnostics;
using IWalker.Util;
using System.Reactive.Subjects;
using Windows.Data.Pdf;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_PDFFile
    {

        [TestMethod]
        public async Task DownloadFileNoP()
        {
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();

            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            vm.DownloadOrUpdate.Execute(null);

            await pf.WhenAny(x => x.NumberOfPages, x => x.Value)
                .Where(x => x != 0)
                .Timeout(TimeSpan.FromSeconds(1), Observable.Return<int>(0))
                .FirstAsync();

            Assert.AreEqual(10, pf.NumberOfPages);
        }

        [TestMethod]
        public async Task DownloadFileFromCacheNotThereYet()
        {
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                throw new InvalidOperationException();
            };

            // Install original data in cache
            var dc = new dummyCache();
            await dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, data)).FirstAsync();

            // Create VM's and hook them up.
            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            // Start it off
            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(10, pf.NumberOfPages);
        }

        // Was ok
        [TestMethod]
        public async Task CheckCacheLookupHappensOnce()
        {
            // When we have an item that is cached, we shouldn't do the full load
            // too many times, (once) when looking at only the page number.
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                throw new InvalidOperationException();
            };

            // Install original data in cache
            var dc = new dummyCache();
            await dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, data)).FirstAsync();

            // Create VM's and hook them up.
            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            // Start it off
            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(10, pf.NumberOfPages);

            // Now, make sure we did it only once.
            // TODO: Currently this is 2 b.c. there is a second lookup for a date, which also includes
            // going after the file data. This should get fixed and split the database up.
            Assert.AreEqual(3, dc.NumberTimesGetCalled);
        }

        [TestMethod]
        public async Task CheckTest2()
        {
            var data = await TestUtils.GetFileAsBytes("test2.pdf");
            var f = new dummyFile();
            f.GetStream = () =>
            {
                throw new InvalidOperationException();
            };

            // Install original data in cache
            var dc = new dummyCache();
            await dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, data)).FirstAsync();

            // Create VM's and hook them up.
            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            // Start it off
            vm.DownloadOrUpdate.Execute(null);
            await pf.WhenAny(x => x.NumberOfPages, y => y.Value)
                .Where(y => y != 0)
                .Take(1)
                .Timeout(TimeSpan.FromSeconds(1), Observable.Return(0))
                .FirstAsync();

            Assert.AreEqual(6, pf.NumberOfPages);

            // Now, make sure that we still get a "1" out of the update guy.
        }

        [TestMethod]
        public async Task CachedFileGetsUpdated()
        {
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            var data2 = await TestUtils.GetFileAsBytes("test2.pdf");

            var sender = new Subject<StreamReader>();

            var f = new dummyFile();
            f.GetStream = () =>
            {
                return sender;
            };

            // Install original data in cache
            var dc = new dummyCache();
            await dc.InsertObject(f.UniqueKey, Tuple.Create("old date", data)).FirstAsync();

            // Create VM's and hook them up.
            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            // Start it off
            vm.DownloadOrUpdate.Execute(null);
            await pf.WhenAny(x => x.NumberOfPages, y => y.Value)
                .Do(v => Debug.WriteLine("Got value {0} for pages {1}", v, pf.NumberOfPages))
                .Where(y => y != 0)
                .Take(1)
                .Timeout(TimeSpan.FromSeconds(1), Observable.Return(0))
                .FirstAsync();
            Assert.AreEqual(10, pf.NumberOfPages);
            Debug.WriteLine("Got the first file through");

            // Next, do the next download and fetch the values.
            Debug.WriteLine("Sending a new stream reader");
            sender.OnNext(new StreamReader(new MemoryStream(data2)));
            Debug.WriteLine("New stream reader is sent");
            sender.OnCompleted();
            await pf.WhenAny(x => x.NumberOfPages, y => y.Value)
                .Where(y => y != 10)
                .Take(1)
                .Timeout(TimeSpan.FromSeconds(2), Observable.Return(0))
                .FirstAsync();
            Assert.AreEqual(6, pf.NumberOfPages);
        }

        [TestMethod]
        public async Task MonitorPageUpdate()
        {
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            vm.DownloadOrUpdate.Execute(null);

            await pf.WhenAny(x => x.NumberOfPages, y => y.Value)
                .Where(y => y != 0)
                .Take(1)
                .Timeout(TimeSpan.FromSeconds(1), Observable.Return(0))
                .FirstAsync();
            Assert.AreEqual(10, pf.NumberOfPages);

            var pupdate = await pf.GetPageStreamAndCacheInfo(5).FirstAsync();
            var page = await pupdate.Item2.FirstAsync();
            Assert.AreEqual(5, (int) page.Index);
        }

        [TestMethod]
        public async Task GetImagesViaCacheSequence()
        {
            // We want to make sure that if there is a cache image we never try to load
            // the file. This involves just getting the cache key, and if that doesn't
            // cause a fetch, we are good.

            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                throw new InvalidOperationException();
            };

            // For this to work, we need the # of pages in the cache already.
            var dc = new dummyCache();
            await dc.InsertObject(f.UniqueKey, Tuple.Create("old date", data)).FirstAsync();
            var dtc = await dc.GetObjectCreatedAt<Tuple<string, byte[]>>(f.UniqueKey).FirstAsync();

            var cacheStem = string.Format("talk.pdf-{0}", dtc.Value.ToString());
            await dc.InsertObject(cacheStem + "-NumberOfPages", 10).FirstAsync();

            // Create VM's and hook them up.
            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            // Get the cache info and the items from it, we are really only
            // interested in the first item.
            var cacheInfo = await pf
                .GetPageStreamAndCacheInfo(5)
                .Timeout(TimeSpan.FromSeconds(2), Observable.Return<Tuple<string, IObservable<PdfPage>>>(null))
                .FirstAsync();

            Assert.IsNotNull(cacheInfo);
            Assert.AreEqual(cacheStem, cacheInfo.Item1);

            Assert.AreEqual(1, dc.NumberTimesGetCalled);
        }

        [TestMethod]
        public async Task GetFileViaCacheSequenceTwice()
        {
            // Pretend a cache miss, and fetch the file to do the render. ANd then
            // do it again.
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            vm.DownloadOrUpdate.Execute(null);

            await pf.WhenAny(x => x.NumberOfPages, y => y.Value)
                .Where(y => y != 0)
                .Take(1)
                .Timeout(TimeSpan.FromSeconds(1), Observable.Return(0))
                .FirstAsync();
            Assert.AreEqual(10, pf.NumberOfPages);

            var pupdate = await pf.GetPageStreamAndCacheInfo(5).FirstAsync();

            // First rendering
            var page = await pupdate.Item2.FirstAsync();

            // Get the # of times we've done a data lookup. And this shouldn't change when we get it again.
            var getCalls = dc.NumberTimesGetCalled;
            Debug.WriteLine("Get was called {0} times after first page was rendered.", getCalls);
            var pageAgain = await pupdate.Item2.FirstAsync();
            Assert.AreEqual(getCalls, dc.NumberTimesGetCalled);
        }

    }
}
