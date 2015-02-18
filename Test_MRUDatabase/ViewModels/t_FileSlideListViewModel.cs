using Akavache;
using IWalker.DataModel.Interfaces;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWalker.ViewModels;
using System.IO;
using System.Reactive.Linq;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_FileSlideListViewModel
    {
        [TestInitialize]
        public void Setup()
        {
            BlobCache.ApplicationName="Test_MRUDatabase";
            BlobCache.UserAccount.InvalidateAll();
            BlobCache.UserAccount.Flush();
        }

        [TestMethod]
        public async Task TestNumberTimesFileRequested()
        {
            // Make sure the # of times a file is loaded from disk is reasonable.
            var df = new dummmyFile("test.pdf", "test.pdf");
            var vm = new FileSlideListViewModel(df);

            var list = vm.SlideThumbnails;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);

            await vm.DoneBuilding.FirstAsync();

            Assert.AreEqual(10, list.Count);
            Assert.AreEqual(1, df.Called);
        }

        // A dummmy file.
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
        }
    }
}
