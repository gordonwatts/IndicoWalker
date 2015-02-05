using IWalker.DataModel.Interfaces;
using IWalker.DataModel.MRU;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Threading.Tasks;

namespace Test_MRUDatabase
{
    [TestClass]
    public class BasicUsage
    {
        [TestMethod]
        public void CTor()
        {
            var c = new MRUDatabaseAccess();
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public async Task AddMeeting()
        {
            var c = new MRUDatabaseAccess();
            var m = GenerateSimpleMeeting();
            await c.MarkVisitedNow(m);
        }

        /// <summary>
        /// Generate a reall simple meeting locally.
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
