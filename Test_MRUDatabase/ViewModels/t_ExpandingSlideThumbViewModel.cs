using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    /// <summary>
    /// Test the expanding VM.
    /// </summary>
    [TestClass]
    public class t_ExpandingSlideThumbViewModel
    {
        [TestMethod]
        public async Task ExpandDosentHappenAutomatically()
        {
            // Open a single talk and see if we can see it open.
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));

            // In this test we make sure not to access the # of pages.
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            var pf = new PDFFile(vm);

            var exp = new ExpandingSlideThumbViewModel(pf, now);

            // Ok, now look to make sure it isn't expanded yet.
            await TestUtils.SpinWait(() => exp.TalkAsThumbs != null, 100, false);
            Assert.IsNull(exp.TalkAsThumbs);
        }

        [TestMethod]
        public async Task ShowSlidesIsAllowed()
        {
            // Open a single talk and see if we can see it open.
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));

            // In this test we make sure not to access the # of pages.
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            var pf = new PDFFile(vm);

            var exp = new ExpandingSlideThumbViewModel(pf, now);

            // Make sure we are allowed to show the guys
            await TestUtils.SpinWait(() => exp.CanShowThumbs == true, 500);
        }

        [TestMethod]
        public async Task ShowSlidesInOne()
        {
            // Open a single talk and see if we can see it open.
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));

            // In this test we make sure not to access the # of pages.
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            var pf = new PDFFile(vm);

            var exp = new ExpandingSlideThumbViewModel(pf, now);

            // Make sure there are no slides - also primes the pump for Rx.
            Assert.IsNull(exp.TalkAsThumbs);

            // Open the slides!
            exp.ShowSlides.Execute(null);

            // See if they opened!
            await TestUtils.SpinWait(() => exp.TalkAsThumbs != null, 5000);
        }

#if later
        [TestMethod]
        public async Task ButtonNotGoodTillDownload()
        {
            // Make sure that while a file is downloading that the button to show the slides doesn't work
            Assert.Inconclusive();
        }
#endif
    }
}
