using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.IO;
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
        public void Setup()
        {
            BlobCache.ApplicationName="Test_MRUDatabase";
            BlobCache.UserAccount.InvalidateAll();
            BlobCache.UserAccount.Flush();
        }

        [TestMethod]
        public async Task TestForNoFile()
        {
            var df = new dummmyFile("test.pdf", "test");
            var f = df.GetFile(false);
            string r = null;
            f.Subscribe(
                a => r = "SHould not have gotten anything",
                e => r = "Failed with exception: " + e.Message
                );

            Assert.AreEqual(null, r);
            Assert.AreEqual(0, df.Called);
        }

        [TestMethod]
        public async Task TestForGettingFile()
        {
            var df = new dummmyFile("test.pdf", "test");
            var f = df.GetFile(true);
            var raStream = await f;
            Assert.IsNotNull(raStream);
            Assert.AreEqual(1, df.Called);
        }

        [TestMethod]
        public async Task GetFileOfCorrectLength()
        {
            var df = new dummmyFile("test.pdf", "test") as IFile;
            var f = df.GetFile(true);
            var raStream = await f;

            var ms = new MemoryStream();
            await raStream.AsStream().CopyToAsync(ms);

            Assert.AreEqual(1384221, ms.Length);
        }

        [TestMethod]
        public async Task GetFileOfCorrectLengthTwice()
        {
            var df = new dummmyFile("test.pdf", "test") as IFile;
            var f1 = df.GetFile(true);
            var raStream1 = await f1;

            var f2 = df.GetFile(false);
            var raStream = await f2;
            
            var ms = new MemoryStream();
            await raStream.AsStream().CopyToAsync(ms);

            Assert.AreEqual(1384221, ms.Length);
        }

        [TestMethod]
        public async Task WriteSecondFileToDisk()
        {
            var df = new dummmyFile("test.pdf", "test") as IFile;
            var f1 = df.GetFile(true);
            var raStream1 = await f1;

            var f2 = df.GetFile(false);
            var raStream = await f2;

            var outputFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("WriteSecondFileToDisk.pdf");
            using (var s = await outputFile.OpenStreamForWriteAsync())
            {
                await raStream.AsStream().CopyToAsync(s);
            }

            // do the PDF thing and see if the number of pages is right.
            await CheckPDFDocument(outputFile, 10);
        }

        [TestMethod]
        public async Task CheckPDFFromFirstDownload()
        {
            var df = new dummmyFile("test.pdf", "test");
            var f = df.GetFile(true);
            var raStream = await f;
            await CheckPDFDocument(raStream, 10);
        }

        [TestMethod]
        public async Task CheckPDFFromSeconmdDownload()
        {
            var df = new dummmyFile("test.pdf", "test");
            var f = df.GetFile(true);
            var raStream = await f;
            f = df.GetFile(true);
            raStream = await f;
            await CheckPDFDocument(raStream, 10);
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
            var df = new dummmyFile("test.pdf", "test");
            var f1 = df.GetFile(true);
            var raStream1 = await f1;

            var f2 = df.GetFile(false);
            var raStream = await f2;

            Assert.AreEqual(1, df.Called);
        }

        [TestMethod]
        public async Task GetFileFromCacheWithUpdate()
        {
            var df = new dummmyFile("test.pdf", "test");
            var f1 = df.GetFile(true);
            var raStream1 = await f1;

            var f2 = df.GetFile(true);
            var raStream = await f2;

            Assert.AreEqual(1, df.Called);
        }

        class dummmyFile : IFile
        {
            public int Called { get; private set; }
            private  string _name;
            private  string _url;
            public dummmyFile(string url, string name)
            {
                _name = name;
                _url = url;
                Called = 0;
            }

            public bool IsValid { get { return true; } }

            public string FileType { get { return "pdf"; } }

            public string UniqueKey { get { return _name;  } }

            public async Task<StreamReader> GetFileStream()
            {
                Called++;
                var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(_url);
                var reader = await f.OpenStreamForReadAsync();
                return new StreamReader(reader);
            }

            public string DisplayName { get { return _name; } }
        }

    }
}
