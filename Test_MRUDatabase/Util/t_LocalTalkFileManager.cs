using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;


namespace Test_MRUDatabase
{
    [TestClass]
    public class t_LocalTalkFileManager
    {
        [TestInitialize]
        public async Task Setup()
        {
            BlobCache.ApplicationName = "Test_MRUDatabase";
            await Blobs.LocalStorage.InvalidateAll();
            await Blobs.LocalStorage.Flush();
        }

        [TestMethod]
        public void TestForNoFile()
        {
            var df = new dummyFile("test.pdf", "test");
            var f = df.GetFileFromCache(Blobs.LocalStorage);
            string r = null;
            f.Subscribe(
                a => r = "SHould not have gotten anything",
                e => r = "Failed with exception: " + e.Message
                );

            Assert.AreEqual(null, r);
            Assert.AreEqual(0, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task TestForGettingFile()
        {
            var df = new dummyFile("test.pdf", "test");
            var f = await df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();
            Assert.IsNotNull(f);
            Assert.AreEqual(1, f.Count);
            Assert.IsNotNull(f[0]);
            Assert.AreEqual(1, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task GetFileOfCorrectLength()
        {
            var df = new dummyFile("test.pdf", "test") as IFile;
            var f = df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();
            var raStream = await f;

            var ms = new MemoryStream();
            await raStream[0].AsStream().CopyToAsync(ms);

            Assert.AreEqual(1384221, ms.Length);
        }

        [TestMethod]
        public async Task GetFileOfCorrectLengthTwice()
        {
            var df = new dummyFile("test.pdf", "test") as IFile;
            var f1 = df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();
            var raStream1 = await f1;

            var f2 = df.GetFileFromCache(Blobs.LocalStorage)
                .ToList()
                .FirstAsync();

            var raStream = await f2;

            var ms = new MemoryStream();
            await raStream[0].AsStream().CopyToAsync(ms);

            Assert.AreEqual(1384221, ms.Length);
        }

        [TestMethod]
        public async Task WriteSecondFileToDisk()
        {
            var df = new dummyFile("test.pdf", "test") as IFile;
            var f1 = df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();
            var raStream1 = await f1;

            var f2 = df.GetFileFromCache(Blobs.LocalStorage)
                .ToList()
                .FirstAsync();
            var raStream = await f2;

            var outputFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("WriteSecondFileToDisk.pdf");
            using (var s = await outputFile.OpenStreamForWriteAsync())
            {
                await raStream[0].AsStream().CopyToAsync(s);
            }

            // do the PDF thing and see if the number of pages is right.
            await CheckPDFDocument(outputFile, 10);
        }

        [TestMethod]
        public async Task CheckPDFFromFirstDownload()
        {
            var df = new dummyFile("test.pdf", "test");
            var f = df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();
            var raStream = await f;
            await CheckPDFDocument(raStream[0], 10);
        }

        [TestMethod]
        public async Task CheckPDFFromSeconmdDownload()
        {
            var df = new dummyFile("test.pdf", "test");
            var f = df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();
            var raStream = await f;
            f = df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();
            raStream = await f;
            await CheckPDFDocument(raStream[0], 10);
        }

        [TestMethod]
        public async Task CheckCachedUpdateWorks()
        {
            var df = new dummyFile("test.pdf", "test");
            var f = await df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();

            df.DateToReturn = "forget me not";
            f = await df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();

            Assert.AreEqual(2, f.Count);
        }

        [TestMethod]
        public async Task UpdateSequenceNoChanges()
        {
            var df = new dummyFile("test.pdf", "test");
            // There is a race condition in the Akavache - when the requests come too fast, the SQL insertion can't keep up.
            // Not obvious how to "lock things out".
            var f = await df.GetAndUpdateFileUponRequest(Observable.Interval(TimeSpan.FromMilliseconds(300)).Take(2).Select(_ => default(Unit)))
                .ToList()
                .FirstAsync();

            Assert.AreEqual(1, f.Count);
        }

        [TestMethod]
        public async Task UpdateSequenceWithChanges()
        {
            var df = new dummyFile("test.pdf", "test");
            var seq = Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(_ => 1)
                .Take(2)
                .Scan((a, b) => a + b)
                .Select(index => new string[] { "1", "2" }[index - 1])
                .Do(s => df.DateToReturn = s)
                .Select(_ => default(Unit));


            var f = await df.GetAndUpdateFileUponRequest(seq)
                .ToList()
                .FirstAsync();

            Assert.AreEqual(2, f.Count);
        }

        [TestMethod]
        public async Task CheckCacheUpdateNotRequired()
        {
            var df = new dummyFile("test.pdf", "test");
            var f = await df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();

            f = await df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();

            Assert.AreEqual(1, f.Count);
        }

        private async Task CheckPDFDocument(Windows.Storage.Streams.IRandomAccessStream raStream, uint nPages)
        {
            var doc = await PdfDocument.LoadFromStreamAsync(raStream);
            Assert.AreEqual(nPages, doc.PageCount);
        }

        /// <summary>
        /// Check the number of pages from a storage file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="nPages"></param>
        /// <returns></returns>
        async Task CheckPDFDocument(StorageFile file, uint nPages)
        {
            var doc = await PdfDocument.LoadFromFileAsync(file);
            Assert.AreEqual(nPages, doc.PageCount);
        }


        [TestMethod]
        public async Task GetFileFromCache()
        {
            var df = new dummyFile("test.pdf", "test");
            var f1 = df.GetAndUpdateFileOnce();
            var raStream1 = await f1;

            var f2 = df.GetFileFromCache(Blobs.LocalStorage);
            var raStream = await f2;

            Assert.AreEqual(1, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task GetFileFromCacheWithUpdate()
        {
            var df = new dummyFile("test.pdf", "test");
            var f1 = df.GetAndUpdateFileOnce();
            var raStream1 = await f1;

            var f2 = df.GetAndUpdateFileOnce();
            var raStream = await f2;

            Assert.AreEqual(1, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task CreationTimeNothingCreated()
        {
            var df = new dummyFile("test.pdf", "test");
            var r = await df.GetCacheCreateTime();
            Assert.IsNull(r);
        }

        [TestMethod]
        public async Task CreatingTimeAfterCache()
        {
            var df = new dummyFile("test.pdf", "test");
            var f1 = await df.GetAndUpdateFileOnce();

            var r = await df.GetCacheCreateTime();
            Assert.IsNotNull(r);
        }

        [TestMethod]
        public async Task UpdateOnceNoCache()
        {
            var df = new dummyFile("test.pdf", "test");
            var f = await df.UpdateFileOnce();
            Assert.IsNotNull(f);
            Assert.AreEqual(1, df.GetStreamCalled);
            // Make sure it has been cached as well.
            var o = await Blobs.LocalStorage.GetObject<Tuple<string, byte[]>>(df.UniqueKey);
            Assert.IsNotNull(o);
            Assert.AreEqual(await df.GetFileDate(), o.Item1);
        }

        [TestMethod]
        public async Task UpdateOnceWithCache()
        {
            var df = new dummyFile("test.pdf", "test");
            await df.GetAndUpdateFileOnce();

            var f = await df.UpdateFileOnce()
                .ToList()
                .FirstAsync();
            Assert.IsNotNull(f);
            Assert.AreEqual(0, f.Count);
        }

        [TestMethod]
        public async Task UpdateOnceWithCacheAndUpdate()
        {
            var df = new dummyFile("test.pdf", "test");
            await df.GetAndUpdateFileOnce();
            df.DateToReturn = "must update now";

            var f = await df.UpdateFileOnce()
                .ToList()
                .FirstAsync();
            Assert.IsNotNull(f);
            Assert.AreEqual(1, f.Count);
        }
    }
}
