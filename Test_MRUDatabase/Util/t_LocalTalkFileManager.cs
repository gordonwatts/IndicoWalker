using Akavache;
using IWalker.Util;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
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
        public async Task CreationTimeNothingCreated()
        {
            var df = new dummyFile("test.pdf", "test");
            var r = await df.GetCacheCreateTime();
            Assert.IsNull(r);
        }
    }
}
