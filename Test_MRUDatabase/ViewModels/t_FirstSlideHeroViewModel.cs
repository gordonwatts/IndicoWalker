using IWalker.ViewModels;
using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_FirstSlideHeroViewModel
    {
        [TestInitialize]
        public void Setup()
        {
            FileDownloadController.Reset();
        }

        [TestMethod]
        public async Task WaitForFirstSlideReady()
        {
            // Get the dummy file and input real PDF data.
            // Hook it up to the download controller
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();
            var fdc = new FileDownloadController(f, dc);

            var pf = new PDFFile(fdc);
            var dummy1 = pf.NumberOfPages;

            // The first spin guy
            var hero = new FirstSlideHeroViewModel(pf, null);
            Assert.IsNull(hero.HeroPageUC);

            // Run the download
            fdc.DownloadOrUpdate.Execute(null);

            // Make sure the thing is ready now.
            await TestUtils.SpinWait(() => hero.HeroPageUC != null, 1000);
        }

        [TestMethod]
        public async Task WaitForFirstSlideAfterReady()
        {
            // Get the dummy file and input real PDF data.
            // Hook it up to the download controller
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };

            var dc = new dummyCache();
            var fdc = new FileDownloadController(f, dc);

            var pf = new PDFFile(fdc);
            var dummy1 = pf.NumberOfPages;

            // Run the download
            fdc.DownloadOrUpdate.Execute(null);
            await TestUtils.SpinWait(() => pf.NumberOfPages != 0, 1000);

            // The first spin guy
            var hero = new FirstSlideHeroViewModel(pf, null);

            // Make sure the thing is ready now.
            await TestUtils.SpinWait(() => hero.HeroPageUC != null, 1000);
        }

        [TestMethod]
        public void InputFileIsNull()
        {
            var hero = new FirstSlideHeroViewModel((PDFFile) null, null);
            Assert.IsFalse(hero.HaveHeroSlide);
            Assert.IsNull(hero.HeroPageUC);
        }
    }
}
