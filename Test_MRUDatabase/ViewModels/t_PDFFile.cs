﻿using IWalker.ViewModels;
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

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_PDFFile
    {

        [TestMethod]
        public async Task DownloadFileNoP()
        {
            await new TestScheduler().With(async sched =>
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
                    .FirstAsync();

                Assert.AreEqual(10, pf.NumberOfPages);
            });
        }

#if false
        [TestMethod]
        public async Task DownloadFileFromCacheNoPNotThereYet()
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
            bool gotit = false;
            pf.PDFDocumentUpdated.Subscribe(_ => gotit = true);

            // Start it off
            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(10, pf.NumberOfPages);
            Assert.IsTrue(gotit);
        }

#endif
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
            Assert.AreEqual(2, dc.NumberTimesGetCalled);
        }

#if false
        [TestMethod]
        public async Task DownloadedFiresEvenWhenLateSubscribe()
        {
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            GC.Collect();
            Debug.WriteLine("Using test.pdf which is {0} bytes long.", data.Length);

            await new TestScheduler().With(async sched =>
            {
                var f = new dummyFile();
                f.GetStream = () =>
                {
                    throw new InvalidOperationException();
                };

                // Install original data in cache
                var dc = new dummyCache();
                await dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, data)).FirstAsync();
                GC.Collect();

                // Create VM's and hook them up.
                var vm = new FileDownloadController(f, dc);
                var pf = new PDFFile(vm);
                var dummy1 = pf.NumberOfPages;
                GC.Collect();

                // Start it off
                vm.DownloadOrUpdate.Execute(null);
                sched.AdvanceByMs(100);
                GC.Collect();
                await pf.WhenAny(x => x.NumberOfPages, y => y.Value)
                    .Do(y => Debug.WriteLine("Looking at a page count of {0}", y))
                    .Where(y => y != 0)
                    .Take(1)
                    .WriteLine("Got a number of pages - time to sign off")
                    .FirstAsync();
                Assert.AreEqual(10, pf.NumberOfPages);

                // Now, make sure that we still get a "1" out of the update guy.
                bool goit = false;
                pf.PDFDocumentUpdated.Subscribe(_ => goit = true);
                Assert.IsTrue(goit);
            });
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
                .FirstAsync();

            Assert.AreEqual(6, pf.NumberOfPages);

            // Now, make sure that we still get a "1" out of the update guy.
            bool goit = false;
            pf.PDFDocumentUpdated.Subscribe(_ => goit = true);
            Assert.IsTrue(goit);
            GC.Collect();
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

            pf.PDFDocumentUpdated.Subscribe(_ => Debug.WriteLine("Just got a PDF update: {0}", pf.NumberOfPages));

            // Start it off
            vm.DownloadOrUpdate.Execute(null);
            await pf.WhenAny(x => x.NumberOfPages, y => y.Value)
                .Do(v => Debug.WriteLine("Got value {0} for pages {1}", v, pf.NumberOfPages))
                .Where(y => y != 0)
                .Take(1)
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
                .FirstAsync();
            Assert.AreEqual(6, pf.NumberOfPages);
        }
#endif
#if false
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

            Assert.AreEqual(10, pf.NumberOfPages);

            var pupdate = await pf.GetPageStream(1).FirstAsync();
            Assert.AreEqual(1, (int) pupdate.Index);
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

            // Install original data in cache
            var dc = new dummyCache();
            await dc.InsertObject(f.UniqueKey, Tuple.Create(f.DateToReturn, data)).FirstAsync();

            // Create VM's and hook them up.
            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;
            bool gotit = false;
            pf.PDFDocumentUpdated.Subscribe(_ => gotit = true);

            // Start it off
            vm.DownloadOrUpdate.Execute(null);

            // Get the cache info and the items from it, we are really only
            // interested in the first item.
            var cacheInfo = await pf.GetPageStreamAndCacheInfo().FirstAsync();

            Assert.AreEqual("bogus", cacheInfo.Item2);

            // Now, make sure that we've not tried to render or download the PDF file.
            Assert.IsFalse(gotit);
        }
#endif

        [TestMethod]
        public async Task GetFIleViaCacheSequence()
        {
            // Pretend a cache miss, and fetch the file to do the render.
            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task GetFIleViaCacheSequenceTwice()
        {
            // Pretend a cache miss, and fetch the file to do the render. ANd then
            // do it again.
            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task GetFileViaUpdateCache()
        {
            // We get an image, then a file update occurs, and we get the new file and do the "render".
            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task GetNumberOfPagesWhenCached()
        {
            // If we have cached the number of pages for this file, then we shouldn't
            // need to render at all.
            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task NumberCacheLookupsSmall()
        {
            // Examine log files and make sure all those cache lookups are actually needed.
            Assert.Inconclusive();
        }

    }
}
