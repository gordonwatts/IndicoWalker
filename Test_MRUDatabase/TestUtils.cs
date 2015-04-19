using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;

namespace Test_MRUDatabase
{
    static class TestUtils
    {
        /// <summary>
        /// Load up a PDF document.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static async Task<PdfDocument> GetPDF(string p)
        {
            var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(p);
            var reader = await f.OpenStreamForReadAsync();
            var pdf = await PdfDocument.LoadFromStreamAsync(System.IO.WindowsRuntimeStreamExtensions.AsRandomAccessStream(reader));
            return pdf;
        }

        /// <summary>
        /// Read a file in and return it as a set of bytes.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetFileAsBytes(string p)
        {
            var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(p);
            var len = (int) (await f.GetBasicPropertiesAsync()).Size;
            var data = new byte[len];
            using (var reader = await f.OpenStreamForReadAsync())
            {

                var bytesRead = await reader.ReadAsync(data, 0, len);

                if (bytesRead != len)
                    throw new InvalidOperationException();

                return data;
            }
        }
    }
}
