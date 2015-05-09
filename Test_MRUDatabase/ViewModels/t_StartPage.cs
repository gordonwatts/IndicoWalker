using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using Test_MRUDatabase.Util;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_StartPage
    {
        [TestMethod]
        public void CTor()
        {
            var ds = new dummyScreen();
            var t = new StartPageViewModel(ds);
        }

        [TestMethod]
        public void LoadNormalMeeting()
        {
            var ds = new dummyScreen();
            var t = new StartPageViewModel(ds);

            object newPage = null;
            ds.Router.Navigate.Subscribe(o => newPage = o);

            t.MeetingAddress = "https://indico.cern.ch/event/377091/";
            t.SwitchPages.Execute(null);

            Assert.IsNotNull(newPage);
            Assert.IsInstanceOfType(newPage, typeof(MeetingPageViewModel));
        }

#if false
        [TestMethod]
        public void LoadBadURL()
        {
            var ds = new dummyScreen();
            var t = new StartPageViewModel(ds);

            object newPage = null;
            ds.Router.Navigate.Subscribe(o => newPage = o);

            t.MeetingAddress = "http://www.nytimes.com";
            t.SwitchPages.Execute(null);

            Assert.IsNull(newPage);
        }
#endif

        [TestMethod]
        public void LoadCategory()
        {
            var ds = new dummyScreen();
            var t = new StartPageViewModel(ds);

            object newPage = null;
            ds.Router.Navigate.Subscribe(o => newPage = o);

            t.MeetingAddress = "https://indico.cern.ch/export/categ/1l12.ics?from=-7d";
            t.SwitchPages.Execute(null);

            Assert.IsNotNull(newPage);
            Assert.IsInstanceOfType(newPage, typeof(CategoryPageViewModel));
        }


    }
}
