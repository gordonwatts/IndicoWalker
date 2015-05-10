using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using Splat;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Test_MRUDatabase.Util;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_CategoryPageViewModel
    {
        [TestInitialize]
        public async Task Setup()
        {
            BlobCache.ApplicationName = "Test_MRUDatabase";
            await Blobs.LocalStorage.InvalidateAll();
            await Blobs.LocalStorage.Flush();
            Locator.CurrentMutable.Register(() => new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            }, typeof(JsonSerializerSettings), null);
        }

        [TestMethod]
        public void CTor()
        {
            var ds = new dummyScreen();
            var ms = new myMeetingListRef();
            var t = new CategoryPageViewModel(ds, ms);
        }

        [TestMethod]
        public async Task FetchOnce()
        {
            // When not in cache, make sure it is fetched and updated in the cache.
            var ds = new dummyScreen();
            var ms = new myMeetingListRef();
            var dc = new dummyCache();
            var t = new CategoryPageViewModel(ds, ms, dc);

            await TestUtils.SpinWait(() => dc.NumberTimesInsertCalled >= 1, 1000);

            var item = await dc.GetObject<IMeetingRefExtended[]>(ms.UniqueString);
            Assert.IsNotNull(item);
            Assert.AreEqual(2, item.Length);
            Assert.AreEqual("meeting1", item[0].Title);
            Assert.AreEqual("meeting2", item[1].Title);
        }
    }
}
