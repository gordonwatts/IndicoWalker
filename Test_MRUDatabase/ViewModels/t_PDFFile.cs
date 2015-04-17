using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_PDFFile
    {
        [TestMethod]
        public async Task DownloadFile()
        {
            var f = new dummyFile();
            var data = await TestUtils.GetFileAsBytes("test.pdf");
            f.GetStream = () =>
            {
                return Observable.Return(new StreamReader(new MemoryStream(data)));
            };
            var dc = new dummyCache();
            var vm = new FileDownloadController(f, dc);

            var pf = new PDFFile(vm);
            var dummy1 = pf.NumberOfPages;

            vm.DownloadOrUpdate.Execute(null);

            Assert.AreEqual(10, pf.NumberOfPages);
        }
    }
}
