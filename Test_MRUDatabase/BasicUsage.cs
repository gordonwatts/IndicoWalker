using IWalker.DataModel;
using IWalker.DataModel.Interfaces;
using IWalker.DataModel.MRU;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Threading.Tasks;
using System.Linq;
using SQLite;

namespace Test_MRUDatabase
{
    [TestClass]
    public class BasicUsage
    {
        [TestInitialize]
        public async Task Setup()
        {
            await DBTestHelpers.DeleteDB();
        }

        [TestMethod]
        public void CTor()
        {
            var c = new MRUDatabaseAccess();
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public async Task AddMeeting()
        {
            // Add a single meeting, and get back that meeting.
            var c = new MRUDatabaseAccess();
            var m = GenerateSimpleMeeting();
            await c.MarkVisitedNow(m);
            Assert.AreEqual(1, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        [TestMethod]
        public async Task MakeSureEmpty()
        {
            // Test that what we use to make sure we have an empty database does, indeed, render an empty database.

            // Add a single meeting, and get back that meeting.
            var c = new MRUDatabaseAccess();
            var m = GenerateSimpleMeeting();
            await c.MarkVisitedNow(m);

            await DBTestHelpers.DeleteDB();
            Assert.AreEqual(0, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        /// <summary>
        /// Generate a really simple meeting locally.
        /// </summary>
        /// <returns></returns>
        IMeeting GenerateSimpleMeeting()
        {
            return new dummyMeeting();
        }

        /// <summary>
        /// Dummy meeting to be used above.
        /// </summary>
        class dummyMeeting : IMeeting
        {
            public string Title
            {
                get { return "meeting"; }
            }

            public ISession[] Sessions
            {
                get { return new ISession[0]; }
            }

            public System.DateTime StartTime
            {
                get { return DateTime.Now; }
            }
        }

    }
}
