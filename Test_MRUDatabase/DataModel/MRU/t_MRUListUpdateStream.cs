using IWalker.DataModel;
using IWalker.DataModel.Interfaces;
using IWalker.DataModel.MRU;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase.DataModel.MRU
{
    /// <summary>
    /// Run tests against the MRU updater
    /// </summary>
    [TestClass]
    public class t_MRUListUpdateStream
    {
        /// <summary>
        /// Reset everything.
        /// </summary>
        /// <returns></returns>
        [TestInitialize]
        public async Task Setup()
        {
            await DBTestHelpers.DeleteDB();
            MRUSettingsCache.ResetCache();
            MRUListUpdateStream.Reset();
        }

        [TestMethod]
        public async Task MRUListSetupFirstSubscribeEmpty()
        {
            var dummyCache = await GetFirstMRUList();
            Assert.AreEqual(0, dummyCache.Length);
        }

        [TestMethod]
        public async Task MRUListSetupSecondSubscribeEmpty()
        {
            var dummyCache = await GetFirstMRUList();
            dummyCache = await GetFirstMRUList();
            Assert.AreEqual(0, dummyCache.Length);
        }

        [TestMethod]
        public async Task MRUFromDB()
        {
            // Setup the DB first, and make sure the MRU list returns that item
            // when we go for it.
            await LoadDB(1);

            var dummyCache = await GetFirstMRUList();
            Assert.AreEqual(1, dummyCache.Length);
        }

        [TestMethod]
        public async Task MRUFromDBAfterUpdate()
        {
            // Setup the DB first, and make sure the MRU list returns that item
            // when we go for it.
            await LoadDB(1);
            var dummyCache = await GetFirstMRUList();

            await LoadDB(2, 1);
            dummyCache = await GetFirstMRUList();
            Assert.AreEqual(3, dummyCache.Length);
        }

#if false
        [TestMethod]
        public async Task MRUSLimited()
        {
            Assert.Inconclusive();
            // Make sure we get back only 20 even when weird stuff happens.
        }

        [TestMethod]
        public async Task MRUSorted()
        {
            Assert.Inconclusive();
            // Make sure they come back in the right order
        }
#endif

        /// <summary>
        /// Return the MRU list that is first off the presses.
        /// </summary>
        /// <returns></returns>
        private static async Task<IWalker.MRU[]> GetFirstMRUList()
        {
            var s = await MRUListUpdateStream.GetMRUListStream();
            IWalker.MRU[] dummyCache = null;
            using (var tmp = s.Subscribe(lst => dummyCache = lst))
            {
                await TestUtils.SpinWait(() => dummyCache != null, 1000);
            }

            return dummyCache;
        }

        /// <summary>
        /// Load our db with a few items
        /// </summary>
        /// <param name="nMeetings"></param>
        private async Task LoadDB(int nMeetings, int indexStart = 0)
        {
            var mlst = GenerateMeetings(indexStart)
                .Take(nMeetings);

            var db = new MRUDatabaseAccess();

            foreach (var m in mlst)
            {
                await db.MarkVisitedNow(m);
            }
        }

        /// <summary>
        /// Generate an infinite stream of meetings
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IMeeting> GenerateMeetings(int indexStart)
        {
            int index = indexStart;
            while (true)
            {
                var dm = new dummyEmptyMeeting();
                dm.Title = $"Meeting {index}";
                dm.StartTime = DateTime.Now + TimeSpan.FromHours(index);
                index++;
                yield return dm;
            }
        }
    }
}
