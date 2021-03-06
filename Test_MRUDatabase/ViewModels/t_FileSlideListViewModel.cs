﻿using Akavache;
using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_FileSlideListViewModel
    {
        [TestInitialize]
        public async Task Setup()
        {
            BlobCache.ApplicationName = "Test_MRUDatabase";
            await Blobs.LocalStorage.InvalidateAll();
            await Blobs.LocalStorage.Flush();
            Settings.AutoDownloadNewMeeting = true;
        }

        [TestMethod]
        public async Task TestNumberTimesFileRequested()
        {
            // Make sure the # of times a file is loaded from disk is reasonable.

            var df = new dummyFile("test.pdf", "test.pdf");
            var dfctl = new FileDownloadController(df);
            var pdfFile = new PDFFile(dfctl);
            var vm = new FileSlideListViewModel(pdfFile, new TimePeriod(DateTime.Now, DateTime.Now));

            // Initially, everything should be all zeros.
            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            // Trigger the initial download.
            dfctl.DownloadOrUpdate.Execute(null);

            await TestUtils.SpinWait(() => list.Count == 10, 200);

            Assert.AreEqual(10, list.Count);
            Assert.AreEqual(1, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task TestFileUpdated()
        {
            // First, we need to get the file into the cache. Use the infrastructure to do that.

            var df = new dummyFile("test.pdf", "test.pdf");
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            await df.SaveFileInCache(df.DateToReturn, data, Blobs.LocalStorage);

            // Now, we are going to update the cache, and see if it gets re-read.
            df.DateToReturn = "this is the second one";
            var dfctl = new FileDownloadController(df);
            var pdfFile = new PDFFile(dfctl);
            var vm = new FileSlideListViewModel(pdfFile, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            dfctl.DownloadOrUpdate.Execute(null);
            await TestUtils.SpinWait(() => list.Count != 0, 200);
            await Task.Delay(10);

            Assert.AreEqual(10, list.Count);

            // 2 - when we do the update.
            Assert.AreEqual(1, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task TestFileNoAutoDownload()
        {
            // Cached file, even if no auto upload, should be checked against the internet.

            Settings.AutoDownloadNewMeeting = false;

            var df = new dummyFile("test.pdf", "test.pdf");
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            await df.SaveFileInCache(df.DateToReturn, data, Blobs.LocalStorage);

            // Now, we are going to update the cache, and see if it gets re-read (which it should since we have it)
            df.DateToReturn = "this is the second one";
            var dfctl = new FileDownloadController(df);
            var pdfFile = new PDFFile(dfctl);
            var vm = new FileSlideListViewModel(pdfFile, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
            dfctl.DownloadOrUpdate.Execute(null);

            await TestUtils.SpinWait(() => list.Count != 0, 200);
            await Task.Delay(10);
            Assert.AreEqual(10, list.Count);

            Assert.AreEqual(1, df.GetStreamCalled);
        }

        [TestMethod]
        public void TestFileNoAutoNoCache()
        {
            // First, we need to get the file into the cache. Use the infrastructure to do that.

            Settings.AutoDownloadNewMeeting = false;

            var df = new dummyFile("test.pdf", "test.pdf");

            // Now, we are going to update the cache, and see if it gets re-read (which it should since we have it)
            df.DateToReturn = "this is the second one";
            var dfctl = new FileDownloadController(df);
            var pdfFile = new PDFFile(dfctl);
            var vm = new FileSlideListViewModel(pdfFile, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            Assert.AreEqual(0, list.Count);

            Assert.AreEqual(0, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task TestFileNotUpdated()
        {
            // First, we need to get the file into the cache. Use the infrastructure to do that.

            var df = new dummyFile("test.pdf", "test.pdf");
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            await df.SaveFileInCache(df.DateToReturn, data, Blobs.LocalStorage);

            // Now, we are going to update the cache, and see if it gets re-read.
            var dfctl = new FileDownloadController(df);
            var pdfFile = new PDFFile(dfctl);
            var vm = new FileSlideListViewModel(pdfFile, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            Assert.AreEqual(0, df.GetStreamCalled);
        }

        [TestMethod]
        public async Task LookAtAllSlides()
        {
            // Subscribe to all the slides and get back MemoryStreams for all of them
            // to stress out the simultaneous reading of everything.

            var df = new dummyFile("test.pdf", "test.pdf");
            var dfctl = new FileDownloadController(df);
            var pdfFile = new PDFFile(dfctl);
            var vm = new FileSlideListViewModel(pdfFile, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;

            // Listen for everything to be rendered.
            var allImages = list
                .Select(item => item.PDFPageVM.ImageStream.Where(i => i != null))
                .Select(strm => strm.FirstAsync())
                .Select(async stream => await stream)
                .ToArray();

            // Fire off the rendering of everything
            var renderOptions = Tuple.Create(PDFPageViewModel.RenderingDimension.Horizontal, (double)150, (double)150);
            foreach (var item in list)
            {
                item.PDFPageVM.RenderImage.Execute(renderOptions);
            }

            // And now wait for everything.
            await Task.WhenAll(allImages);
            Assert.IsTrue(allImages.Select(item => item.Result).All(itm => itm != null));
        }

        [TestMethod]
        public async Task FetchSlideOnlyOnce()
        {
            // Subscribe to all the slides and get back MemoryStreams for all of them
            // to stress out the simultaneous reading of everything.

            var df = new dummyFile("test.pdf", "test.pdf");
            var dfctl = new FileDownloadController(df);
            dfctl.DownloadOrUpdate.Execute(null);
            var pdfFile = new PDFFile(dfctl);
            var vm = new FileSlideListViewModel(pdfFile, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;

            // Listen for everything to be rendered.
            var allImages = list
                .Select(item => item.PDFPageVM.ImageStream.Where(i => i != null))
                .Select(strm => strm.FirstAsync())
                .Select(async stream => await stream)
                .ToArray();

            // Fire off the rendering of everything
            var renderOptions = Tuple.Create(PDFPageViewModel.RenderingDimension.Horizontal, (double)150, (double)150);
            foreach (var item in list)
            {
                item.PDFPageVM.RenderImage.Execute(renderOptions);
            }

            // And now wait for everything.
            await Task.WhenAll(allImages);

            // Make sure we had to download the file only once.
            Assert.AreEqual(1, df.GetStreamCalled);
        }
    }
}
