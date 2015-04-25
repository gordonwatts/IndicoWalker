using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_PDFPageViewModel
    {

        [TestMethod]
        public async Task MakeSureNothingRenderedWhenNoImage()
        {
            // The exact image we need is in the cache. So we should never make a
            // request to load the PDF file or PdfDocument.

            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task RenderNormalPdfImageHorizontal()
        {
            // Normal sequence of things when there is no image cached.
            // WHen this VM isn't attached to a actual View, we should not
            // trigger any loading or similar. All should remain very quiet.
            // (e.g. lazy loading).

            // Get the infrastructure setup
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            int timesLoaded = 0;
            f.GetStream = () =>
            {
                timesLoaded++;
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();

            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);

            // Now, build the VM

            var pdfVM = new PDFPageViewModel(pf.GetPageStreamAndCacheInfo(1), dc);

            // Subscribe so we can "get" the image.
            MemoryStream lastImage = null;
            pdfVM.ImageStream.Subscribe(img => lastImage = img);
            Assert.IsNull(lastImage);

            // Render, and make sure things "worked"
            pdfVM.RenderImage.Execute(Tuple.Create(IWalker.ViewModels.PDFPageViewModel.RenderingDimension.Horizontal, (double)100, (double)100));
            vm.DownloadOrUpdate.Execute(null);

            await Task.Delay(2000);
            Assert.AreEqual(1, timesLoaded);
            Assert.AreEqual(3, dc.NumberTimesGetCalled); // Once for data, once for size cache, and once again for data file.
            Assert.IsNotNull(lastImage);
        }

        [TestMethod]
        public async Task RenderNormalRenderEarlyTrigger()
        {
            // Normal sequence of things when there is no image cached.
            // WHen this VM isn't attached to a actual View, we should not
            // trigger any loading or similar. All should remain very quiet.
            // (e.g. lazy loading).

            // Get the infrastructure setup
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            int timesLoaded = 0;
            f.GetStream = () =>
            {
                timesLoaded++;
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();

            var vm = new FileDownloadController(f, dc);

            // It shouldn't matter where the download is triggered from - let it happen early
            // here before other things are hooked up.
            vm.DownloadOrUpdate.Execute(null);
            var pf = new PDFFile(vm);

            // Now, build the VM

            var pdfVM = new PDFPageViewModel(pf.GetPageStreamAndCacheInfo(1), dc);

            // Subscribe so we can "get" the image.
            MemoryStream lastImage = null;
            pdfVM.ImageStream.Subscribe(img => lastImage = img);
            Assert.IsNull(lastImage);

            // Render, and make sure things "worked"
            pdfVM.RenderImage.Execute(Tuple.Create(IWalker.ViewModels.PDFPageViewModel.RenderingDimension.Horizontal, (double)100, (double)100));

            await Task.Delay(2000);
            Assert.AreEqual(1, timesLoaded);
            Assert.AreEqual(3, dc.NumberTimesGetCalled); // Once for data, once for size cache, and once again for data file.
            Assert.IsNotNull(lastImage);
        }

        [TestMethod]
        public async Task RenderAlreadyCachedFile()
        {
            // If the file has already been downloaded and installed locally (on a previous
            // look) then PDF rendering should happen automatically this time, even if
            // the download isn't triggered.

            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task LoadVMButNoImageNoLoad()
        {
            // WHen this VM isn't attached to a actual View, we should not
            // trigger any loading or similar. All should remain very quiet.
            // (e.g. lazy loading).

            // Get the infrastructure setup
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            int timesLoaded = 0;
            f.GetStream = () =>
            {
                timesLoaded++;
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();

            var vm = new FileDownloadController(f, dc);
            var pf = new PDFFile(vm);

            // Now, build the VM

            var pdfVM = new PDFPageViewModel(pf.GetPageStreamAndCacheInfo(1), dc);

            Assert.AreEqual(0, timesLoaded);
        }

        [TestMethod]
        public async Task LoadVMButNoImageLoad()
        {
            // WHen this VM isn't attached to a actual View, we should not
            // trigger any loading or similar. All should remain very quiet.
            // (e.g. lazy loading).

            // Get the infrastructure setup
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            int timesLoaded = 0;
            f.GetStream = () =>
            {
                timesLoaded++;
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();

            var vm = new FileDownloadController(f, dc);
            vm.DownloadOrUpdate.Execute(null);
            var pf = new PDFFile(vm);

            // Now, build the VM

            var pdfVM = new PDFPageViewModel(pf.GetPageStreamAndCacheInfo(1), dc);

            Assert.AreEqual(1, timesLoaded);
            Assert.AreEqual(0, dc.NumberTimesGetCalled);
        }

        [TestMethod]
        public async Task LoadVMButNoImageLoadWithRenderRequeset()
        {
            // WHen this VM isn't attached to a actual View, we should not
            // trigger any loading or similar. All should remain very quiet.
            // (e.g. lazy loading).

            // Get the infrastructure setup
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            int timesLoaded = 0;
            f.GetStream = () =>
            {
                timesLoaded++;
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();

            var vm = new FileDownloadController(f, dc);
            vm.DownloadOrUpdate.Execute(null);
            var pf = new PDFFile(vm);

            // Now, build the VM

            var pdfVM = new PDFPageViewModel(pf.GetPageStreamAndCacheInfo(1), dc);

            // Do a render, but nothing should happen since we've not subscribed to the image list.
            pdfVM.RenderImage.Execute(Tuple.Create(IWalker.ViewModels.PDFPageViewModel.RenderingDimension.Horizontal, 100, 100));

            Assert.AreEqual(1, timesLoaded);
            Assert.AreEqual(0, dc.NumberTimesGetCalled);
        }

#if false
        [TestMethod]
        public async Task TestUIMethod()
        {
            await RunOnUI(async () =>
            {
                await Task.Factory.StartNew(() =>
                {
                    var b = new BitmapImage();
                });
            });
        }
#endif

        /// <summary>
        /// Run on the UI thread.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public async Task RunOnUI(Func<Task> a)
        {
            Exception failure = null;
            bool finished = false;

            await Windows.ApplicationModel.Core.CoreApplication
                .MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        a().Wait();
                        finished = true;
                    }
                    catch (Exception e)
                    {
                        failure = e;
                    }
                });

            if (failure != null)
                throw failure;

            Assert.IsTrue(finished);
        }
    }
}
