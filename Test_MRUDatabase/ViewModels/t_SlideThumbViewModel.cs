using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using Windows.UI.Xaml.Media.Imaging;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_SlideThumbViewModel
    {
        [TestMethod]
        public async Task ImageGeneration()
        {
            var pdf = await GetPDF("test.pdf");
            var page = pdf.GetPage(1);
            var st = new SlideThumbViewModel(page, null, 1);

            // Now, go after the image.
            var imageModelVM = st.PDFPageVM;

            imageModelVM.RenderImage.Execute(Tuple.Create(PDFPageViewModel.RenderingDimension.Horizontal, (double) 150, (double) 150));

            var v = await st.PDFPageVM.ImageStream.Where(i => i != null).FirstAsync();
            Assert.IsNotNull(v);
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
