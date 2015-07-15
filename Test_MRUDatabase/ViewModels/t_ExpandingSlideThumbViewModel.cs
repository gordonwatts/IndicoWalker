using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// Make sure that the thing doesn't open on its own.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ExpandDosentHappenAutomatically()
        {
            // Open a single talk and see if we can see it open.
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));
            var pf = (await MakeDownloaders(1))[0];

            var exp = new ExpandingSlideThumbViewModel(pf, now);

            // Ok, now look to make sure it isn't expanded yet.
            await TestUtils.SpinWait(() => exp.TalkAsThumbs != null, 100, false);
            Assert.IsNull(exp.TalkAsThumbs);
        }

        /// <summary>
        /// Generate a number of MakeDownloaders.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static async Task<PDFFile[]> MakeDownloaders(int count)
        {
            // In this test we make sure not to access the # of pages.

            Func<Task<PDFFile>> creator = async () =>
            {
                var f = new dummyFile();
                var data = await TestUtils.GetFileAsBytes("test.pdf");
                f.GetStream = () =>
                {
                    return Observable.Return(new StreamReader(new MemoryStream(data)));
                };
                var dc = new dummyCache();
                var vm = new FileDownloadController(f, dc);
                vm.DownloadOrUpdate.Execute(null);

                return new PDFFile(vm);
            };

            List<PDFFile> r = new List<PDFFile>();
            for (int i = 0; i < count; i++)
            {
                r.Add(await creator());
            }
            return r.ToArray();
        }

        /// <summary>
        /// Make sure that with the given file downloaded, we can show slides.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ShowSlidesIsAllowed()
        {
            // Open a single talk and see if we can see it open.
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));
            var pf = (await MakeDownloaders(1))[0];

            var exp = new ExpandingSlideThumbViewModel(pf, now);

            // Make sure we are allowed to show the guys
            await TestUtils.SpinWait(() => exp.CanShowThumbs == true, 500);
        }

        /// <summary>
        /// Make sure we actually show the slides.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ShowSlidesInOne()
        {
            // Open a single talk and see if we can see it open.
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));
            var pf = (await MakeDownloaders(1))[0];

            var exp = new ExpandingSlideThumbViewModel(pf, now);

            // Make sure there are no slides - also primes the pump for Rx.
            Assert.IsNull(exp.TalkAsThumbs);
            await TestUtils.SpinWait(() => exp.CanShowThumbs == true, 1000);

            // Open the slides!
            exp.ShowSlides.Execute(null);

            // See if they opened!
            await TestUtils.SpinWait(() => exp.TalkAsThumbs != null, 5000);

            // And the can show thumbs should now be false.
            await TestUtils.SpinWait(() => exp.CanShowThumbs == false, 100);
        }

        /// <summary>
        /// WHen we open the second talk, the first one should be closed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ShowSlidesInSecondTalk()
        {
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));
            var pf = await MakeDownloaders(2);
            var exps = pf.Select(pdf => new ExpandingSlideThumbViewModel(pdf, now)).ToArray();

            // Initialize the download guys
            Assert.IsNull(exps[0].TalkAsThumbs);
            Assert.IsNull(exps[1].TalkAsThumbs);

            // Open the first talk. Second talk should do nothing.
            exps[0].ShowSlides.Execute(null);
            await TestUtils.SpinWait(() => exps[0].TalkAsThumbs != null, 5000);
            Assert.IsNull(exps[1].TalkAsThumbs);

            // Open the second talk. First one should close.
            exps[1].ShowSlides.Execute(null);
            await TestUtils.SpinWait(() => exps[1].TalkAsThumbs != null, 5000);
            await TestUtils.SpinWait(() => exps[0].TalkAsThumbs == null, 1000);
        }

        /// <summary>
        /// As long as the file hasn't been downloaded, mark the button as not yet availible.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ButtonNotGoodTillDownload()
        {
            // Build a PDF file that will only download after we ask it to.
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            var pdf = new PDFFile(vm);


            // Open a single talk and see if we can see it open.
            var now = new TimePeriod(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(1000));

            var exp = new ExpandingSlideThumbViewModel(pdf, now);

            // Make sure there are no slides - also primes the pump for Rx.
            Assert.IsNull(exp.TalkAsThumbs);
            await TestUtils.SpinWait(() => exp.CanShowThumbs == false, 1000);

            // Now, make sure that things go "true" after we fire off the file.
            vm.DownloadOrUpdate.Execute(null);
            await TestUtils.SpinWait(() => exp.CanShowThumbs == true, 1000);
        }
    }
}
