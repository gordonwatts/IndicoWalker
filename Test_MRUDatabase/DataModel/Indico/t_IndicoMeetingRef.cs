using IndicoInterface.NET.SimpleAgendaDataModel;
using IWalker.DataModel.Inidco;
using IWalker.DataModel.Interfaces;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
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
            var fileInfo = new TalkMaterial()
            {
                URL = "http://indico.cern.ch/event/336571/session/1/contribution/1/material/slides/0.pdf"
            };
            var f = new IndicoMeetingRef.IndicoFile(fileInfo, "thisIsAFile") as IFile;

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

        [TestMethod]
        public void TalkFileUrlIsAMess()
        {
            var mess = new Talk()
            {
                Title = "this is a talk",
                StartDate = DateTime.Now - TimeSpan.FromMinutes(30),
                EndDate = DateTime.Now + TimeSpan.FromMinutes(30),
                ID = "5",
                SlideURL = "https://indico.fnal.gov/getFile.py/access?contribId=13&sessionId=0&resId=0&materialId=slides&confId=9726",
                Speakers = new string[] { "G. Watts", "M. Verdu", "R. Upton" },
                TalkType = TypeOfTalk.Talk,
                FilenameExtension = ".pdf",
                DisplayFilename = "dude"
            };

            var mr = new IndicoMeetingRef.IndicoTalk(mess, "t1");
            Assert.AreEqual("pdf", mr.TalkFile.FileType);
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
                Speakers = new string[] { "G. Watts", "M. Verdu", "R. Upton" },
                TalkType = TypeOfTalk.Talk
            };
        }

        [TestMethod]
        public void TalkHasNoMaterial()
        {
            var mess = new Talk()
            {
                Title = "this is a talk",
                StartDate = DateTime.Now - TimeSpan.FromMinutes(30),
                EndDate = DateTime.Now + TimeSpan.FromMinutes(30),
                ID = "5",
                SlideURL = null,
                Speakers = new string[] { },
                TalkType = TypeOfTalk.Talk,
                FilenameExtension = null,
                DisplayFilename = null
            };
            var mr = new IndicoMeetingRef.IndicoTalk(mess, "t1");
            Assert.AreEqual("", mr.TalkFile.FileType);
        }
    }
}
