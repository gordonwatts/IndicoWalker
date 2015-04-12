using IWalker.DataModel.MRU;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Linq;

namespace Test_MRUDatabase.DataModel.MRU
{
    /// <summary>
    /// test out the mru setting cache
    /// </summary>
    [TestClass]
    public class t_MRUSettingsCache
    {
        [TestInitialize]
        public void ResetCache()
        {
            MRUSettingsCache.ResetCache();
        }

        [TestMethod]
        public void GetBackSpecificCacheEmpty()
        {
            var mrus = MRUSettingsCache.GetFromMachine("MACHINE1");
            Assert.IsNull(mrus);
        }

        [TestMethod]
        public void SetSpecificCache()
        {
            var mrus = GenerateMRUs();
            MRUSettingsCache.UpdateForMachine("MACHINE1", mrus);
            var rtn = MRUSettingsCache.GetFromMachine("MACHINE1");
            Assert.AreEqual(mrus.Length, rtn.Length);
            foreach (var dual in mrus.Zip(rtn, (m, r) => Tuple.Create(m, r)))
            {
                Assert.AreEqual(dual.Item1.Id, dual.Item2.Id);
                Assert.AreEqual(dual.Item1.IDRef, dual.Item2.IDRef);
                Assert.AreEqual(dual.Item1.LastLookedAt, dual.Item2.LastLookedAt);
                Assert.AreEqual(dual.Item1.StartTime, dual.Item2.StartTime);
                Assert.AreEqual(dual.Item1.Title, dual.Item2.Title);
            }
        }

        [TestMethod]
        public void UpdateSpecificCache()
        {
            var mrus = GenerateMRUs(5, 0);
            MRUSettingsCache.UpdateForMachine("MACHINE1", mrus);
            mrus = GenerateMRUs(10, 100);
            MRUSettingsCache.UpdateForMachine("MACHINE1", mrus);
            var rtn = MRUSettingsCache.GetFromMachine("MACHINE1");
            Assert.AreEqual(10, rtn.Length);
            Assert.AreEqual(100, rtn[0].Id);
        }

        [TestMethod]
        public void NewDoesNotDeleteOld()
        {
            var mrus = GenerateMRUs(5);
            MRUSettingsCache.UpdateForMachine("MACHINE1", mrus);
            MRUSettingsCache.UpdateForMachine("MACHINE2", mrus);
            var rtn = MRUSettingsCache.GetFromMachine("MACHINE1");
            Assert.AreEqual(5, rtn.Length);
        }

        [TestMethod]
        public void EmptyCache()
        {
            var mrus = MRUSettingsCache.GetAllMachineMRUMeetings();
            Assert.AreEqual(0, mrus.Length);
        }

        [TestMethod]
        public void SingleListInCache()
        {
            var mrus = GenerateMRUs();
            MRUSettingsCache.UpdateForMachine("MACHINE1", mrus);
            var rtn = MRUSettingsCache.GetAllMachineMRUMeetings();
            Assert.AreEqual(10, rtn.Length);
            Assert.AreEqual(mrus[0].IDRef, rtn[0].IDRef);
        }

        [TestMethod]
        public void MergedListNothingCommon()
        {
            var mrus = GenerateMRUs();
            MRUSettingsCache.UpdateForMachine("MACHINE1", mrus);
            mrus = GenerateMRUs(10, 100);
            MRUSettingsCache.UpdateForMachine("MACHINE2", mrus);
            var rtn = MRUSettingsCache.GetAllMachineMRUMeetings();
            Assert.AreEqual(20, rtn.Length);
        }

        [TestMethod]
        public void MergedListCommonItems()
        {
            var mrus = GenerateMRUs();
            MRUSettingsCache.UpdateForMachine("MACHINE1", mrus);
            MRUSettingsCache.UpdateForMachine("MACHINE2", mrus);
            var rtn = MRUSettingsCache.GetAllMachineMRUMeetings();
            Assert.AreEqual(10, rtn.Length);
        }

        [TestMethod]
        public void MergedListKeepRightOne()
        {
            var l1 = new IWalker.MRU[] {
                new IWalker.MRU { Id=10, IDRef = "hiref", Title="this is it", StartTime=DateTime.Now, LastLookedAt=DateTime.Parse("1/20/2010")}
            };
            var l2 = new IWalker.MRU[] {
                new IWalker.MRU { Id=10, IDRef = "hiref", Title="this is not it", StartTime=DateTime.Now, LastLookedAt=DateTime.Parse("1/20/2011")}
            };
            MRUSettingsCache.UpdateForMachine("M1", l1);
            MRUSettingsCache.UpdateForMachine("M2", l2);

            var l = MRUSettingsCache.GetAllMachineMRUMeetings();
            Assert.AreEqual(1, l.Length);
            Assert.AreEqual("this is not it", l[0].Title);
        }

        /// <summary>
        /// Generate some MRU's to store.
        /// </summary>
        /// <returns></returns>
        private IWalker.MRU[] GenerateMRUs(int number = 10, int initialID = 0)
        {
            return Enumerable.Range(initialID, number)
                .Select(id => new IWalker.MRU { Id = id, IDRef = id.ToString(), LastLookedAt = DateTime.Now, StartTime = DateTime.Now, Title = string.Format("Meeting id {0}", id) })
                .ToArray();
        }
    }
}
