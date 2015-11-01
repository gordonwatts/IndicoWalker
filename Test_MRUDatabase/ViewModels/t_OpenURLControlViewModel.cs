using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test_MRUDatabase.Util;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_OpenURLControlViewModel
    {
        [TestMethod]
        public void LoadNormalMeeting()
        {
            var ds = new dummyScreen();
            var t = new OpenURLControlViewModel(ds);

            object newPage = null;
            ds.Router.Navigate.Subscribe(o => newPage = o);

            t.MeetingAddress = "https://indico.cern.ch/event/377091/";
            t.SwitchPages.Execute(null);

            Assert.IsNotNull(newPage);
            Assert.IsInstanceOfType(newPage, typeof(MeetingPageViewModel));
        }

        [TestMethod]
        public void LoadCategory()
        {
            var ds = new dummyScreen();
            var t = new OpenURLControlViewModel(ds);

            object newPage = null;
            ds.Router.Navigate.Subscribe(o => newPage = o);

            t.MeetingAddress = "https://indico.cern.ch/export/categ/1l12.ics?from=-7d";
            t.SwitchPages.Execute(null);

            Assert.IsNotNull(newPage);
            Assert.IsInstanceOfType(newPage, typeof(CategoryPageViewModel));
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
    }
    }
