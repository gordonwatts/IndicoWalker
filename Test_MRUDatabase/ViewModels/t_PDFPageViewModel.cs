using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_PDFPageViewModel
    {
#if false
        // TODO: This test seems to hang for some reason.
        [TestMethod]
        public async Task HorizontalRender()
        {
            var pdf = await GetPDF("test.pdf");
            var pdfVM = new PDFPageViewModel(pdf.GetPage(1));

            // Watch what comes back.
            var lastImagePromise = pdfVM.ImageStream.Where(i => i != null).FirstAsync();

            // Render...
            pdfVM.RenderImage.Execute(Tuple.Create(PDFPageViewModel.RenderingDimension.Horizontal, (double)150, (double)150));

            var lastImage = await lastImagePromise;
            Assert.IsNotNull(pdfVM.ImageStream);
            Assert.IsNotNull(lastImage);
        }

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
                .MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                try {
                    a().Wait();
                    finished = true;
                } catch (Exception e)
                {
                    failure = e;
                }
                });

            if (failure != null)
                throw failure;

            Assert.IsTrue(finished);
        }

        /// <summary>
        /// Load up a PDF document.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private async Task<PdfDocument> GetPDF(string p)
        {
            var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(p);
            var reader = await f.OpenStreamForReadAsync();
            var pdf = await PdfDocument.LoadFromStreamAsync(System.IO.WindowsRuntimeStreamExtensions.AsRandomAccessStream(reader));
            return pdf;
        }
    }
}
