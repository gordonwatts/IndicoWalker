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
        public async Task MRUUpdateCreatesRemoteFile()
        {
            var dummyCache = await GetFirstMRUList();
            await LoadDB(1);

            await TestUtils.SpinWait(() => MRUSettingsCache.GetAllMachineMRUMeetings().Length == 1, 1000, throwIfTimeout: false);

            var result = MRUSettingsCache.GetAllMachineMRUMeetings();
            Assert.AreEqual(1, result.Length);
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

        [TestMethod]
        public void MRUGetMachineName()
        {
            var mn = MRUListUpdateStream.MachineName;
            Assert.IsNotNull(mn);
        }

        [TestMethod]
        public async Task MRUAttemptToSetAfterSetup()
        {
            try
            {
                var dummyCache = await GetFirstMRUList();
                MRUListUpdateStream.MachineName = "MACHINE1";
            } catch (InvalidOperationException e)
            {
                return;
            }

            Assert.Fail("Expected exception not observed");
        }

        [TestMethod]
        public async Task MRUFromOtherMachines()
        {
            // When we have a MRU list on another machine, make sure it shows up here
            // as expected.
            GenerateOtherMachineMRU("MACHINE1", 10);

            var dummyCache = await GetFirstMRUList();
            Assert.AreEqual(10, dummyCache.Length);
        }

        [TestMethod]
        public async Task MRUOtherMachineUpdate()
        {
            // Add the MRU list from another machine at a later time and
            // make sure it shows up.
            var dummyCache = await GetFirstMRUList();
            GenerateOtherMachineMRU("MACHINE1", 10);

            dummyCache = await GetFirstMRUList(10);
        }

        [TestMethod]
        public async Task MRUOtherMachineCombine()
        {
            // Add the same meeting in two different places,
            // make sure we get the one back we want.

            // First, the remote machine.
            GenerateOtherMachineMRU("MACHINE1", 10);

            // Now, this one, and put the times in the future
            await LoadDB(10, 0, TimeSpan.FromHours(1));

            var dummyCache = await GetFirstMRUList(10);

            var youngest = dummyCache.Select(m => m.LastLookedAt).OrderBy(k => k).First();
            var minutesDifferent = youngest - DateTime.Now;
            Assert.IsTrue(minutesDifferent.Minutes >= 59, $"Youngest date {youngest} was too close to {DateTime.Now} (it was {minutesDifferent.Minutes} minutes apart).");
        }

        #if false
        // We will undo these tests as we go. Mostly written as ideas of what I want to make sure this
        // module conforms to for now.
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

        [TestMethod]
        public async Task UpdatesThatDontChagneDontCauseUpdates()
        {
            Assert.Inconslusive();

            // Do an update that would trigger something, but no data change. The output
            // should not alter and remain quiet.
        }

#endif

        /// <summary>
        /// Return the MRU list that is first off the presses.
        /// </summary>
        /// <returns></returns>
        private static async Task<IWalker.MRU[]> GetFirstMRUList(int sizeMin = 0)
        {
            var s = MRUListUpdateStream.GetMRUListStream();
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
        private async Task LoadDB(int nMeetings, int indexStart = 0, TimeSpan futureTime = new TimeSpan())
        {
            var mlst = GenerateMeetings(indexStart)
                .Take(nMeetings);

            var db = new MRUDatabaseAccess();

            var timestamp = DateTime.Now + futureTime;

            foreach (var m in mlst)
            {
                await db.MarkVisitedNow(m, timestamp);
            }
        }

        /// <summary>
        /// Fill up another machine file
        /// </summary>
        /// <param name="machineName"></param>
        /// <param name="numberOfMRUs"></param>
        private void GenerateOtherMachineMRU(string machineName, int numberOfMRUs)
        {
            MRUSettingsCache.UpdateForMachine(machineName, GenerateMRUs().Take(numberOfMRUs).ToArray());
        }

        /// <summary>
        /// Generate an infinite list of MRUs
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IWalker.MRU> GenerateMRUs(int indexStart = 0)
        {
            return GenerateMeetings(indexStart)
                .Select(m => new IWalker.MRU()
                {
                    IDRef = m.AsReferenceString(),
                    StartTime = m.StartTime,
                    Title = m.Title,
                    LastLookedAt = DateTime.Now
                });
        }

        /// <summary>
        /// Generate an infinite stream of meetings
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IMeeting> GenerateMeetings(int indexStart = 0)
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
