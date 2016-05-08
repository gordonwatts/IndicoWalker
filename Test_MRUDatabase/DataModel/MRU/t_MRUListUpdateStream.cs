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

            // Prime the setup so we can make sure that other stuff shows up even after
            // it has started.
            var dummyCache = await GetFirstMRUList();

            // Load up a further two items by marking them.
            await LoadDB(2, 1);

            dummyCache = await GetFirstMRUList(3);
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

        [TestMethod]
        public async Task MachineFileWritten()
        {
            Assert.Inconclusive();
            // Make sure the per-machine files are written.
            // This test may not actually belong here, however.
        }

#endif

        /// <summary>
        /// Return the MRU list that is first off the presses.
        /// </summary>
        /// <returns></returns>
        private static async Task<IWalker.MRU[]> GetFirstMRUList(int sizeMin = 0)
        {
            var s = await MRUListUpdateStream.GetMRUListStream();
            IWalker.MRU[] dummyCache = null;
            using (var tmp = s.Subscribe(lst => dummyCache = lst))
            {
                await TestUtils.SpinWait(() => dummyCache != null, 1000);
                await TestUtils.SpinWait(() => dummyCache.Length >= sizeMin, 1000);
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
