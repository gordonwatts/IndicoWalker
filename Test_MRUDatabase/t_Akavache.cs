using Akavache;
using IWalker.DataModel.Interfaces;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using Splat;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI.Testing;
using IWalker.Util;

namespace Test_MRUDatabase
{
    /// <summary>
    /// There are some behaviors in Akavache that were rather confusing.
    /// This class contains some methods to make sure things work as we
    /// are expecting them to.
    /// </summary>
    [TestClass]
    public class t_Akavache
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
        public async Task GetAndFetchLatestWithItemsInThere()
        {
            var settings = Locator.Current.GetService<JsonSerializerSettings>();
            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.TypeNameHandling, TypeNameHandling.All);

            var m = new dummyMeetingRef();
            Func<Task<IMeeting>> fetcher = async () =>
            {
                var x = await m.GetMeeting();
                return x;
            };

            await Blobs.LocalStorage.InsertObject(m.AsReferenceString(), await m.GetMeeting());

            var blob = Blobs.LocalStorage.GetAndFetchLatest(m.AsReferenceString(), fetcher);

            var mtg = await blob
                .ToList()
                .FirstAsync();

            Assert.IsNotNull(mtg);
            Assert.AreEqual(2, mtg.Count);
            Assert.IsNotNull(mtg.First());
            Assert.IsNotNull(mtg.Skip(1).First());
        }

        [TestMethod]
        public async Task GetAndFetchLatestWithNoItemsInThere()
        {
            var settings = Locator.Current.GetService<JsonSerializerSettings>();
            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.TypeNameHandling, TypeNameHandling.All);

            var m = new dummyMeetingRef();
            Func<Task<IMeeting>> fetcher = async () =>
            {
                var x = await m.GetMeeting();
                return x;
            };

            var blob = Blobs.LocalStorage.GetAndFetchLatest(m.AsReferenceString(), fetcher);

            var mtg = await blob
                .ToList()
                .FirstAsync();

            Assert.IsNotNull(mtg);
            Assert.AreEqual(1, mtg.Count);
            Assert.IsNotNull(mtg.First());
        }

        [TestMethod]
        public async Task GetAndFetchWithTestScheduler()
        {
            await new TestScheduler().With(async sched =>
            {
                var settings = Locator.Current.GetService<JsonSerializerSettings>();
                Assert.IsNotNull(settings);
                Assert.AreEqual(settings.TypeNameHandling, TypeNameHandling.All);

                var m = new dummyMeetingRef();
                Func<Task<IMeeting>> fetcher = async () =>
                {
                    var x = await m.GetMeeting();
                    return x;
                };

                var blob = Blobs.LocalStorage.GetAndFetchLatest(m.AsReferenceString(), fetcher);

                var mtg = await blob
                    .ToList()
                    .FirstAsync();

                Assert.IsNotNull(mtg);
                Assert.AreEqual(1, mtg.Count);
                Assert.IsNotNull(mtg.First());
            });
        }

    }
}
