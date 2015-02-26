using IndicoInterface.NET.SimpleAgendaDataModel;
using IWalker.DataModel.Inidco;
using IWalker.DataModel.Interfaces;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase.DataModel.Indico
{
    /// <summary>
    /// Test out some of the indico interaction stuff
    /// </summary>
    [TestClass]
    public class t_IndicoMeetingRef
    {
        [TestMethod]
        public async Task HeaderDateOnPublicConference()
        {
            // Make sure that we can get back the header date properly.
            var fileUri = "http://indico.cern.ch/event/336571/session/1/contribution/1/material/slides/0.pdf";
            var f = new IndicoMeetingRef.IndicoFile(fileUri, "thisIsAFile") as IFile;

            Assert.AreEqual("1/28/2015 4:53:19 PM +01:00", await f.GetFileDate());
        }

        [TestMethod]
        public void TalksAreSame()
        {
            var t1 = MakeTalk();
            var t2 = MakeTalk();

            var tt1 = new IndicoMeetingRef.IndicoTalk(t1, "hi");
            var tt2 = new IndicoMeetingRef.IndicoTalk(t2, "hi");

            Assert.IsTrue(tt1.Equals(tt2));
        }

        [TestMethod]
        public void TalkTitlesChange()
        {
            var t1 = MakeTalk();
            var t2 = MakeTalk();
            t2.Title = "second-title";

            var tt1 = new IndicoMeetingRef.IndicoTalk(t1, "hi");
            var tt2 = new IndicoMeetingRef.IndicoTalk(t2, "hi");

            Assert.IsFalse(tt1.Equals(tt2));
        }

        [TestMethod]
        public void TalkEndDateChanges()
        {
            var t1 = MakeTalk();
            var t2 = MakeTalk();
            t2.EndDate = DateTime.Now;

            var tt1 = new IndicoMeetingRef.IndicoTalk(t1, "hi");
            var tt2 = new IndicoMeetingRef.IndicoTalk(t2, "hi");

            Assert.IsFalse(tt1.Equals(tt2));
        }

        [TestMethod]
        public void TalkStartDateChanges()
        {
            var t1 = MakeTalk();
            var t2 = MakeTalk();
            t2.StartDate = DateTime.Now;

            var tt1 = new IndicoMeetingRef.IndicoTalk(t1, "hi");
            var tt2 = new IndicoMeetingRef.IndicoTalk(t2, "hi");

            Assert.IsFalse(tt1.Equals(tt2));
        }

        [TestMethod]
        public void TalkFileChanges()
        {
            var t1 = MakeTalk();
            var t2 = MakeTalk();
            t2.SlideURL = "http://indico.cern.ch";

            var tt1 = new IndicoMeetingRef.IndicoTalk(t1, "hi");
            var tt2 = new IndicoMeetingRef.IndicoTalk(t2, "hi");

            Assert.IsFalse(tt1.Equals(tt2));
        }

        private Talk MakeTalk()
        {
            return new Talk()
            {
                Title = "this is a talk",
                StartDate = DateTime.Now - TimeSpan.FromMinutes(30),
                EndDate = DateTime.Now + TimeSpan.FromMinutes(30),
                ID = "5",
                SlideURL = "https://indico.cern.ch/event/23722/material/0/0.pdf",
                Speakers = new string[] { "G. Watts", "M. Verdu", "R. Upton"},
                TalkType = TypeOfTalk.Talk
            };
        }
    }
}
