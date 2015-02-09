using IWalker.DataModel;
using IWalker.DataModel.Interfaces;
using IWalker.DataModel.MRU;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
            var m = GenerateSimpleMeeting(DateTime.Now);
            await c.MarkVisitedNow(m);
            Assert.AreEqual(1, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        [TestMethod]
        public async Task Add2Meetings()
        {
            // Add two of them.
            var c = new MRUDatabaseAccess();
            var m1 = GenerateSimpleMeeting(DateTime.Now, "meeting 1", 100);
            var m2 = GenerateSimpleMeeting(DateTime.Now, "meeting 2", 101);
            await c.MarkVisitedNow(m1);
            await c.MarkVisitedNow(m2);

            Assert.AreEqual(2, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        [TestMethod]
        public async Task AddMeetingTwice()
        {
            // When we update the MRU, it shouldn't create a new entry! :-)
            var c = new MRUDatabaseAccess();
            var m = GenerateSimpleMeeting(DateTime.Now);
            await c.MarkVisitedNow(m);
            await c.MarkVisitedNow(m);
            Assert.AreEqual(1, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        [TestMethod]
        public async Task Add2MeetingsRepeatFirst()
        {
            // Add two of them, then re-mark the first. Shouldn't change anything!
            var c = new MRUDatabaseAccess();
            var m1 = GenerateSimpleMeeting(DateTime.Now, "meeting 1", 100);
            var m2 = GenerateSimpleMeeting(DateTime.Now, "meeting 2", 101);
            await c.MarkVisitedNow(m1);
            await c.MarkVisitedNow(m2);
            await c.MarkVisitedNow(m1);

            Assert.AreEqual(2, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        [TestMethod]
        public async Task Add2MeetingsRepeatSecond()
        {
            // Add two of them, then re-mark the first. Shouldn't change anything!
            var c = new MRUDatabaseAccess();
            var m1 = GenerateSimpleMeeting(DateTime.Now, "meeting 1", 100);
            var m2 = GenerateSimpleMeeting(DateTime.Now, "meeting 2", 101);
            await c.MarkVisitedNow(m1);
            await c.MarkVisitedNow(m2);
            await c.MarkVisitedNow(m2);

            Assert.AreEqual(2, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        [TestMethod]
        public async Task GetBackValidAgenda()
        {
            // After adding a meeting, we should get back an expected agneda info object.

            var c = new MRUDatabaseAccess();
            var m = GenerateSimpleMeeting(DateTime.Now);
            await c.MarkVisitedNow(m);

            var mru = await (await c.QueryMRUDB()).FirstOrDefaultAsync();
            Assert.IsNotNull(m);

            var ai = mru.ToAgendaInfo();
            Assert.AreEqual("indico.cern.ch/^//^/100", ai.AsReferenceString());
        }

        [TestMethod]
        public async Task MakeSureEmpty()
        {
            // Test that what we use to make sure we have an empty database does, indeed, render an empty database.

            // Add a single meeting, and get back that meeting.
            var c = new MRUDatabaseAccess();
            var m = GenerateSimpleMeeting(DateTime.Now);
            await c.MarkVisitedNow(m);

            await DBTestHelpers.DeleteDB();
            Assert.AreEqual(0, await c.ExecuteScalarAsync<int>("select count(*) from MRU"));
        }

        [TestMethod]
        public async Task GetBackMultipleMeetingsSorted()
        {
            var c = new MRUDatabaseAccess();
            foreach (var db in Enumerable.Range(0, 10).Select(i => GenerateSimpleMeeting(new DateTime(2000, 12, i + 1), string.Format("meeting {0}", i), 100 + i)))
            {
                Debug.WriteLine("Inserting meeting {0}", db.Title);
                await c.MarkVisitedNow(db);
            }

            var allMeetings = await c.QueryMRUDB();
            var firstM = await allMeetings.OrderBy(m => m.StartTime).FirstOrDefaultAsync();
            var lastM = await allMeetings.OrderByDescending(m => m.StartTime).FirstOrDefaultAsync();

            Assert.AreEqual("meeting 0", firstM.Title);
            Assert.AreEqual("meeting 9", lastM.Title);
        }

        [TestMethod]
        public async Task StortByMRU()
        {
            var c = new MRUDatabaseAccess();
            foreach (var db in Enumerable.Range(0, 10).Select(i => GenerateSimpleMeeting(new DateTime(2000, 12, 25 - i), string.Format("meeting {0}", i), 100 + i)))
            {
                Debug.WriteLine("Inserting meeting {0}", db.Title);
                await Task.Delay(10);
                await c.MarkVisitedNow(db);
            }

            var allMeetings = await c.QueryMRUDB();
            var firstM = await allMeetings.OrderBy(m => m.LastLookedAt).FirstOrDefaultAsync();
            var lastM = await allMeetings.OrderByDescending(m => m.LastLookedAt).FirstOrDefaultAsync();

            Assert.AreEqual("meeting 0", firstM.Title);
            Assert.AreEqual("meeting 9", lastM.Title);
        }

        /// <summary>
        /// Generate a really simple meeting locally.
        /// </summary>
        /// <returns></returns>
        IMeeting GenerateSimpleMeeting(DateTime when, string title = "meeting", int id = 100)
        {
            return new dummyMeeting(title, when, id);
        }

        /// <summary>
        /// Dummy meeting to be used above.
        /// </summary>
        class dummyMeeting : IMeeting
        {
            private string _title;
            private DateTime _start;
            int _id;
            public dummyMeeting(string title, DateTime start, int id)
            {
                _title = title;
                _start = start;
                _id = id;
            }
            public string Title
            {
                get { return _title; }
            }

            public ISession[] Sessions
            {
                get { return new ISession[0]; }
            }

            public System.DateTime StartTime
            {
                get { return _start; }
            }


            public string AsReferenceString()
            {
                return (string.Format("indico.cern.ch/^//^/{0}", _id));
            }
        }

    }
}
