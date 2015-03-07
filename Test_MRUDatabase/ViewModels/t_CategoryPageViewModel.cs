using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using Splat;
using System;
using System.Collections.Generic;
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
            var ms = new myMeetings();
            var t = new CategoryPageViewModel(ds, ms);
        }

        [TestMethod]
        public async Task FetchOnce()
        {
            // When not in cache, make sure it is fetched and updated in the cache.
            var ds = new dummyScreen();
            var ms = new myMeetings();
            var t = new CategoryPageViewModel(ds, ms);

            await Task.Delay(5000);
            Assert.AreEqual(1, ms.Counter);

            var item = await Blobs.LocalStorage.GetObject<IMeetingRefExtended[]>(ms.UniqueString);
            Assert.IsNotNull(item);
            Assert.AreEqual(2, item.Length);
            Assert.AreEqual("meetign1", item[0].Title);
            Assert.AreEqual("meeting2", item[1].Title);
        }

        class myMeetings : IMeetingListRef
        {
            public int Counter { get; private set; }

            public myMeetings()
            {
                Counter = 0;
            }

            /// <summary>
            /// Return a dummy set of meetings.
            /// </summary>
            /// <param name="goingBackDays"></param>
            /// <returns></returns>
            public Task<IEnumerable<IMeetingRefExtended>> GetMeetings(int goingBackDays)
            {
                Counter++;
                return Task.Factory.StartNew(() =>
                {
                    return new IMeetingRefExtended[] { new aMeeting("meeting1"), new aMeeting("meeting2") } as IEnumerable<IMeetingRefExtended>;
                });
            }


            public string UniqueString
            {
                get { return "111222"; }
            }
        }

        /// <summary>
        /// Dummy extended meeting for testing.
        /// </summary>
        class aMeeting : IMeetingRefExtended
        {
            public aMeeting(string mname)
            {
                Title = mname;
            }
            public string Title { get; private set; }

            public System.DateTime StartTime { get { return DateTime.Now; } }

            public System.DateTime EndTime { get { return DateTime.Now; } }

            public IMeetingRef Meeting
            {
                get { throw new System.NotImplementedException(); }
            }
        }

    }
}
