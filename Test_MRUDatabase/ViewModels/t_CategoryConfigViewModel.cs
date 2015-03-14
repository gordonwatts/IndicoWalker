using IWalker.DataModel.Categories;
using IWalker.DataModel.Interfaces;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using Splat;
using System.Collections.Generic;
using Windows.Storage;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_CategoryConfigViewModel
    {
        [TestInitialize]
        public void ResetCategoryDB()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("CategoryDBSerliazied"))
                ApplicationData.Current.RoamingSettings.Values.Remove("CategoryDBSerliazied");
            Locator.CurrentMutable.Register(() => new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            }, typeof(JsonSerializerSettings), null);
        }

        /// <summary>
        /// Return true if there is any config info in the db.
        /// </summary>
        /// <returns></returns>
        public static bool HaveDBConfigInfo()
        {
            return ApplicationData.Current.RoamingSettings.Values.ContainsKey("CategoryDBSerliazied");
        }

        private CategoryConfigInfo FindDBConfigInfo(IMeetingListRef meetingListRef)
        {
            throw new System.NotImplementedException();
        }

        [TestMethod]
        public void UnknownNotTouched()
        {
            // Given an unknown item, nothing is done.
            var ccvm = new CategoryConfigViewModel(new myMeetingListRef());
            Assert.IsFalse(HaveDBConfigInfo());
            Assert.IsFalse(ccvm.IsDisplayedOnMainPage);
            Assert.IsFalse(ccvm.IsSubscribed);
            Assert.AreEqual("hi", ccvm.CategoryTitle);
        }

        [TestMethod]
        public void UnknownDisplayed()
        {
            var unknown = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var ccvm = new CategoryConfigViewModel(unknown.MeetingList);
            ccvm.IsDisplayedOnMainPage = true;

            Assert.IsTrue(ccvm.IsSubscribed);
            Assert.IsTrue(ccvm.IsDisplayedOnMainPage);

            Assert.IsTrue(HaveDBConfigInfo());
            var info = FindDBConfigInfo(unknown.MeetingList);
            Assert.IsNotNull(info);
            Assert.IsTrue(info.DisplayOnHomePage);
            Assert.AreEqual("hi", info.CategoryTitle);
        }

        [TestMethod]
        public void UnkonwnSubscribed()
        {
            var unknown = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var ccvm = new CategoryConfigViewModel(unknown.MeetingList);
            ccvm.IsSubscribed = true;

            Assert.IsTrue(ccvm.IsSubscribed);
            Assert.IsTrue(ccvm.IsDisplayedOnMainPage);

            Assert.IsTrue(HaveDBConfigInfo());
            var info = FindDBConfigInfo(unknown.MeetingList);
            Assert.IsNotNull(info);
            Assert.IsTrue(info.DisplayOnHomePage);
            Assert.AreEqual("hi", info.CategoryTitle);
        }

        [TestMethod]
        public void Known()
        {
            var unknown1 = new CategoryConfigInfo() { CategoryTitle = "This", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var list = new List<CategoryConfigInfo>() { unknown1 };
            CategoryDB.SaveCategories(list);

            var unknown2 = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var ccvm = new CategoryConfigViewModel(unknown2.MeetingList);

            Assert.IsTrue(ccvm.IsSubscribed);
            Assert.IsFalse(ccvm.IsDisplayedOnMainPage);
            Assert.AreEqual("This", ccvm.CategoryTitle);
        }

        [TestMethod]
        public void KnownDisplayed()
        {
            var unknown1 = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = true, MeetingList = new myMeetingListRef() };
            var list = new List<CategoryConfigInfo>() { unknown1 };
            CategoryDB.SaveCategories(list);

            var unknown2 = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var ccvm = new CategoryConfigViewModel(unknown2.MeetingList);

            Assert.IsTrue(ccvm.IsSubscribed);
            Assert.IsTrue(ccvm.IsDisplayedOnMainPage);
        }

        [TestMethod]
        public void KnownUnsubscribed()
        {
            var unknown1 = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = true, MeetingList = new myMeetingListRef() };
            var list = new List<CategoryConfigInfo>() { unknown1 };
            CategoryDB.SaveCategories(list);

            var unknown = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var ccvm = new CategoryConfigViewModel(unknown.MeetingList);
            ccvm.IsSubscribed = false;

            var info = FindDBConfigInfo(unknown.MeetingList);
            Assert.IsNull(info);
        }

        [TestMethod]
        public void KnownUndisplayed()
        {
            var unknown1 = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = true, MeetingList = new myMeetingListRef() };
            var list = new List<CategoryConfigInfo>() { unknown1 };
            CategoryDB.SaveCategories(list);

            var unknown = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var ccvm = new CategoryConfigViewModel(unknown.MeetingList);
            ccvm.IsSubscribed = false;

            var info = FindDBConfigInfo(unknown.MeetingList);
            Assert.IsNotNull(info);
            Assert.IsFalse(info.DisplayOnHomePage);
        }

        [TestMethod]
        public void KnownTitleUpdated()
        {
            var unknown1 = new CategoryConfigInfo() { CategoryTitle = "Dude", DisplayOnHomePage = true, MeetingList = new myMeetingListRef() };
            var list = new List<CategoryConfigInfo>() { unknown1 };
            CategoryDB.SaveCategories(list);

            var unknown = new CategoryConfigInfo() { CategoryTitle = "hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef() };
            var ccvm = new CategoryConfigViewModel(unknown.MeetingList);
            ccvm.CategoryTitle = "There";

            var info = FindDBConfigInfo(unknown.MeetingList);
            Assert.IsNotNull(info);
            Assert.AreEqual("There", info.CategoryTitle);
        }
    }
}
