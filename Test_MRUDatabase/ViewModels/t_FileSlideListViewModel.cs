using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWalker.ViewModels;
using System.IO;
using System.Reactive.Linq;
using System.Reactive;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_FileSlideListViewModel
    {
        [TestInitialize]
        public async Task Setup()
        {
            BlobCache.ApplicationName="Test_MRUDatabase";
            await Blobs.LocalStorage.InvalidateAll();
            await Blobs.LocalStorage.Flush();
        }

        [TestMethod]
        public async Task TestNumberTimesFileRequested()
        {
            // Make sure the # of times a file is loaded from disk is reasonable.
            var df = new dummmyFile("test.pdf", "test.pdf");
            var vm = new FileSlideListViewModel(df, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            await vm.DoneBuilding.FirstAsync();

            Assert.AreEqual(10, list.Count);
            Assert.AreEqual(1, df.Called);
        }

        [TestMethod]
        public async Task TestFileUpdated()
        {
            // First, we need to get the file into the cache. Use the infrastructure to do that.

            var df = new dummmyFile("test.pdf", "test.pdf");
            await df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();

            // Now, we are going to update the cache, and see if it gets re-read.
            df.DateToReturn = "this is the second one";
            var vm = new FileSlideListViewModel(df, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            await vm.DoneBuilding.FirstAsync();
            await vm.DoneBuilding.FirstAsync();

            Assert.AreEqual(2, df.Called);
        }

        [TestMethod]
        public async Task TestFileNotUpdated()
        {
            // First, we need to get the file into the cache. Use the infrastructure to do that.

            var df = new dummmyFile("test.pdf", "test.pdf");
            await df.GetAndUpdateFileOnce()
                .ToList()
                .FirstAsync();

            // Now, we are going to update the cache, and see if it gets re-read.
            var vm = new FileSlideListViewModel(df, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            await vm.DoneBuilding.FirstAsync();

            Assert.AreEqual(1, df.Called);
        }

        [TestMethod]
        public async Task LookAtAllSlides()
        {
            // Subscribe to all the slides and get back MemoryStreams for all of them
            // to stress out the simultaneous reading of everything.

            var df = new dummmyFile("test.pdf", "test.pdf");
            var vm = new FileSlideListViewModel(df, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            await vm.DoneBuilding.FirstAsync();

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
            Task.WaitAll(allImages);
            Assert.IsTrue(allImages.Select(item => item.Result).All(itm => itm != null));
        }

        [TestMethod]
        public async Task FetchSlideOnlyOnce()
        {
            // Subscribe to all the slides and get back MemoryStreams for all of them
            // to stress out the simultaneous reading of everything.

            var df = new dummmyFile("test.pdf", "test.pdf");
            var vm = new FileSlideListViewModel(df, new TimePeriod(DateTime.Now, DateTime.Now));

            var list = vm.SlideThumbnails;
            await vm.DoneBuilding.FirstAsync();

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
            Task.WaitAll(allImages);

            // Make sure we had to download the file only once.
            Assert.AreEqual(1, df.Called);
        }

        // A dummy file.
        class dummmyFile : IFile
        {
            public int Called { get; private set; }
            private string _name;
            private string _url;
            public dummmyFile(string url, string name)
            {
                _name = name;
                _url = url;
                Called = 0;
                DateToReturn = "this is the first";
            }

            public bool IsValid { get { return true; } }

            public string FileType { get { return "pdf"; } }

            public string UniqueKey { get { return _name; } }

            public async Task<StreamReader> GetFileStream()
            {
                Called++;
                var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(_url);
                var reader = await f.OpenStreamForReadAsync();
                return new StreamReader(reader);
            }

            public string DisplayName { get { return _name; } }

            /// <summary>
            /// Date stamp to return.
            /// </summary>
            public string DateToReturn { get; set; }
            public Task<string> GetFileDate()
            {
                return Task.Factory.StartNew(() => DateToReturn);
            }
        }
    }
}
